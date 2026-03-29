using FluentValidation;
using UmbLink.Web;
using UmbLink.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Requests;
using UmbLink.Application.Services;
using UmbLink.Application.BackgroundServices;
using UmbLink.Web.Cache;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Identity;
using UmbLink.Infrastructure.Repositories;
using UmbLink.Infrastructure.Seed;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

var builder = WebApplication.CreateBuilder(args);

// Blazor Server (.NET 8)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// EF Core + SQLite
var connStr = builder.Configuration.GetConnectionString("Default") ?? "Data Source=umblink.db";
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(connStr));

// ASP.NET Identity
builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(o =>
{
    o.Password.RequireDigit = true;
    o.Password.RequiredLength = 8;
    o.Password.RequireNonAlphanumeric = false;
    o.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, CustomUserClaimsPrincipalFactory>();

// Auth state for Blazor
builder.Services.AddCascadingAuthenticationState();

// Google OAuth (opcional — só ativa se ClientId estiver configurado)
var googleClientId = builder.Configuration["Google:ClientId"];
if (!string.IsNullOrEmpty(googleClientId))
{
    builder.Services.AddAuthentication()
        .AddGoogle(o =>
        {
            o.ClientId = googleClientId;
            o.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
        });
}

// Authorization policies
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// Background queue + metrics
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<MetricsBackgroundService>();

// Repositories
builder.Services.AddScoped<IPageRepository, PageRepository>();
builder.Services.AddScoped<ILinkRepository, LinkRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IPlanLimitRepository, PlanLimitRepository>();

// Application services
builder.Services.AddScoped<IPlanLimitService, PlanLimitService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<ILinkService, LinkService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IMetricsService, MetricsService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Cache
var redisConn = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConn))
{
    builder.Services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
}
else
{
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
}

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreatePageRequestValidator>();

// Rate limiting no endpoint de tracking
builder.Services.AddRateLimiter(o =>
    o.AddFixedWindowLimiter("tracking", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    }));

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

var app = builder.Build();

// Migrations + Seed automático
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Bloqueio de /admin por middleware (não só por UI)
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/admin"))
    {
        if (!ctx.User.Identity!.IsAuthenticated || !ctx.User.IsInRole("Admin"))
        {
            ctx.Response.StatusCode = 403;
            return;
        }
    }
    await next();
});

// Auth endpoints
app.MapPost("/auth/do-login", async (
    HttpContext ctx,
    SignInManager<AppUser> sm) =>
{
    var form = ctx.Request.Form;
    var email      = form["email"].ToString();
    var password   = form["password"].ToString();
    var remember   = form["remember"] == "true";
    var returnUrl  = form["returnUrl"].ToString();

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        return Results.Redirect("/auth/login?error=required");

    var result = await sm.PasswordSignInAsync(email, password, remember, lockoutOnFailure: true);

    if (result.Succeeded)
        return Results.Redirect(string.IsNullOrEmpty(returnUrl) ? "/dashboard" : returnUrl);

    var code = result.IsLockedOut ? "locked" : "invalid";
    var ret  = string.IsNullOrEmpty(returnUrl) ? "" : $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
    return Results.Redirect($"/auth/login?error={code}{ret}");
}).DisableAntiforgery();

app.MapPost("/auth/do-register", async (
    HttpContext ctx,
    UserManager<AppUser> um,
    SignInManager<AppUser> sm,
    AppDbContext db) =>
{
    var form     = ctx.Request.Form;
    var name     = form["name"].ToString().Trim();
    var email    = form["email"].ToString().Trim();
    var password = form["password"].ToString();

    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        return Results.Redirect("/auth/register?error=required");

    var user = new AppUser { Name = name, Email = email, UserName = email };
    var result = await um.CreateAsync(user, password);
    if (!result.Succeeded)
    {
        var msg = Uri.EscapeDataString(result.Errors.First().Description);
        return Results.Redirect($"/auth/register?error={msg}");
    }

    await um.AddToRoleAsync(user, "User");
    var freePlan = await db.Plans.FirstAsync(p => p.Name == "Free");
    db.Subscriptions.Add(new UmbLink.Infrastructure.Data.Entities.Subscription
    {
        UserId = user.Id,
        PlanId = freePlan.Id,
        Status = UmbLink.Infrastructure.Data.Entities.SubscriptionStatus.Free
    });
    await db.SaveChangesAsync();
    await sm.SignInAsync(user, isPersistent: false);
    return Results.Redirect("/onboarding");
}).DisableAntiforgery();

app.MapPost("/auth/logout", async (SignInManager<AppUser> sm) =>
{
    await sm.SignOutAsync();
    return Results.Redirect("/auth/login");
}).RequireAuthorization();

app.MapGet("/auth/google-login", () =>
    Results.Challenge(
        new AuthenticationProperties { RedirectUri = "/auth/google-callback" },
        ["Google"]));

app.MapGet("/auth/google-callback", async (
    HttpContext ctx,
    UserManager<AppUser> um,
    SignInManager<AppUser> sm,
    AppDbContext db) =>
{
    var info = await sm.GetExternalLoginInfoAsync();
    if (info is null) return Results.Redirect("/auth/login?error=google");

    var result = await sm.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
    if (result.Succeeded) return Results.Redirect("/dashboard");

    var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    if (email is null) return Results.Redirect("/auth/login?error=google");

    var name = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? email;
    var user = new AppUser { Email = email, UserName = email, Name = name };
    var createResult = await um.CreateAsync(user);
    if (!createResult.Succeeded) return Results.Redirect("/auth/login?error=google");

    await um.AddLoginAsync(user, info);
    var freePlan = await db.Plans.FirstAsync(p => p.Name == "Free");
    db.Subscriptions.Add(new UmbLink.Infrastructure.Data.Entities.Subscription
    {
        UserId = user.Id,
        PlanId = freePlan.Id,
        Status = UmbLink.Infrastructure.Data.Entities.SubscriptionStatus.Free
    });
    await db.SaveChangesAsync();
    await sm.SignInAsync(user, false);
    return Results.Redirect("/onboarding");
});

// Track endpoint público (métricas)
app.MapPost("/api/track/view/{pageId:guid}", async (
    Guid pageId, HttpContext ctx, IMetricsService metrics) =>
{
    var referrer = ctx.Request.Headers.Referer.ToString();
    await metrics.TrackViewAsync(pageId, string.IsNullOrEmpty(referrer) ? null : referrer);
    return Results.Ok();
}).RequireRateLimiting("tracking");

app.MapPost("/api/track/click/{linkId:guid}", async (
    Guid linkId, HttpContext ctx, IMetricsService metrics) =>
{
    var ua = ctx.Request.Headers.UserAgent.ToString();
    var referrer = ctx.Request.Headers.Referer.ToString();
    await metrics.TrackClickAsync(linkId,
        string.IsNullOrEmpty(ua) ? null : ua[..Math.Min(150, ua.Length)],
        string.IsNullOrEmpty(referrer) ? null : referrer);
    return Results.Ok();
}).RequireRateLimiting("tracking");

// Click tracking redirect
app.MapGet("/r/{linkId:guid}", async (Guid linkId, IMetricsService metrics,
    IPageService pageSvc, HttpContext ctx) =>
{
    var link = await pageSvc.GetLinkByIdAsync(linkId);
    if (link is null) return Results.Redirect("/");
    var ua = ctx.Request.Headers.UserAgent.ToString();
    var referrer = ctx.Request.Headers.Referer.ToString();
    await metrics.TrackClickAsync(linkId,
        string.IsNullOrEmpty(ua) ? null : ua[..Math.Min(150, ua.Length)],
        string.IsNullOrEmpty(referrer) ? null : referrer);

    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";

    // mailto: always redirect directly
    if (link.Url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
        return Results.Redirect(link.Url);

    // Check if host is in known-safe domains (including subdomains like username.substack.com)
    static bool IsKnownDomain(string host) =>
        RedirectKnownDomains.KnownDomains.Contains(host) ||
        RedirectKnownDomains.KnownDomains.Any(d => host.EndsWith("." + d, StringComparison.OrdinalIgnoreCase));

    if (Uri.TryCreate(link.Url, UriKind.Absolute, out var parsedUri))
    {
        var host = parsedUri.Host;
        if (IsKnownDomain(host))
            return Results.Redirect(link.Url);
    }

    return Results.Redirect($"/redirect-warning?url={Uri.EscapeDataString(link.Url)}");
}).RequireRateLimiting("tracking");

// Avatar upload endpoint
app.MapPost("/api/upload/avatar", [Microsoft.AspNetCore.Authorization.Authorize] async (HttpContext ctx, IWebHostEnvironment env) =>
{
    var userId = ctx.User.GetUserId();
    if (userId == Guid.Empty) return Results.Unauthorized();

    if (!ctx.Request.HasFormContentType) return Results.BadRequest(new { error = "Multipart form expected" });
    var form = await ctx.Request.ReadFormAsync();
    var file = form.Files.GetFile("file");
    if (file is null || file.Length == 0) return Results.BadRequest(new { error = "No file provided" });
    if (file.Length > 2 * 1024 * 1024) return Results.BadRequest(new { error = "File too large (max 2 MB)" });

    var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
    if (!allowed.Contains(file.ContentType.ToLowerInvariant()))
        return Results.BadRequest(new { error = "Invalid file type. Allowed: jpeg, png, webp" });

    // Copy to MemoryStream and validate magic bytes
    using var ms = new MemoryStream();
    await file.OpenReadStream().CopyToAsync(ms);
    ms.Position = 0;
    var buffer = ms.GetBuffer();
    var bytesRead = (int)Math.Min(ms.Length, 12);
    bool isJpeg = bytesRead >= 3 && buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
    bool isPng  = bytesRead >= 8 && buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47;
    bool isWebP = bytesRead >= 12 && buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46
               && buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50;
    if (!isJpeg && !isPng && !isWebP)
        return Results.BadRequest(new { error = "Formato de imagem não reconhecido." });
    ms.Position = 0;

    var dir = Path.Combine(env.WebRootPath, "uploads", "avatars");
    Directory.CreateDirectory(dir);

    // Delete previous avatar for this user
    foreach (var old in Directory.GetFiles(dir, $"{userId}_*.webp"))
        File.Delete(old);

    var fileName = $"{userId}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.webp";
    var outputPath = Path.Combine(dir, fileName);

    using var image = await SixLabors.ImageSharp.Image.LoadAsync(ms);
    var ratio = 400.0 / Math.Min(image.Width, image.Height);
    var newW = (int)(image.Width * ratio);
    var newH = (int)(image.Height * ratio);
    image.Mutate(x => x
        .Resize(newW, newH)
        .Crop(new SixLabors.ImageSharp.Rectangle((newW - 400) / 2, (newH - 400) / 2, 400, 400)));
    await image.SaveAsWebpAsync(outputPath, new SixLabors.ImageSharp.Formats.Webp.WebpEncoder { Quality = 80 });

    return Results.Ok(new { url = $"/uploads/avatars/{fileName}" });
}).DisableAntiforgery().RequireRateLimiting("tracking");

// Background image upload endpoint
app.MapPost("/api/upload/background", [Microsoft.AspNetCore.Authorization.Authorize] async (HttpContext ctx, IWebHostEnvironment env) =>
{
    var userId = ctx.User.GetUserId();
    if (userId == Guid.Empty) return Results.Unauthorized();

    var pageIdStr = ctx.Request.Query["pageId"].ToString();
    if (!Guid.TryParse(pageIdStr, out var pageId))
        return Results.BadRequest(new { error = "Invalid pageId" });

    // Ownership check: ensure the page belongs to the authenticated user
    var pageService = ctx.RequestServices.GetRequiredService<IPageService>();
    var userPages = await pageService.GetUserPagesAsync(userId);
    if (!userPages.Any(p => p.Id == pageId))
        return Results.Forbid();

    if (!ctx.Request.HasFormContentType) return Results.BadRequest(new { error = "Multipart form expected" });
    var form = await ctx.Request.ReadFormAsync();
    var file = form.Files.GetFile("file");
    if (file is null || file.Length == 0) return Results.BadRequest(new { error = "No file provided" });
    if (file.Length > 2 * 1024 * 1024) return Results.BadRequest(new { error = "File too large (max 2 MB)" });

    var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
    if (!allowed.Contains(file.ContentType.ToLowerInvariant()))
        return Results.BadRequest(new { error = "Invalid file type. Allowed: jpeg, png, webp" });

    // Copy to MemoryStream and validate magic bytes
    using var ms = new MemoryStream();
    await file.OpenReadStream().CopyToAsync(ms);
    ms.Position = 0;
    var buffer = ms.GetBuffer();
    var bytesRead = (int)Math.Min(ms.Length, 12);
    bool isJpeg = bytesRead >= 3 && buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
    bool isPng  = bytesRead >= 8 && buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47;
    bool isWebP = bytesRead >= 12 && buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46
               && buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50;
    if (!isJpeg && !isPng && !isWebP)
        return Results.BadRequest(new { error = "Formato de imagem não reconhecido." });
    ms.Position = 0;

    var dir = Path.Combine(env.WebRootPath, "uploads", "backgrounds");
    Directory.CreateDirectory(dir);

    // Delete previous background for this page
    foreach (var old in Directory.GetFiles(dir, $"{pageId}_*.webp"))
        File.Delete(old);

    var fileName = $"{pageId}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.webp";
    var outputPath = Path.Combine(dir, fileName);

    using var image = await SixLabors.ImageSharp.Image.LoadAsync(ms);
    var ratio = Math.Max(1200.0 / image.Width, 800.0 / image.Height);
    var newW = (int)(image.Width * ratio);
    var newH = (int)(image.Height * ratio);
    image.Mutate(x => x
        .Resize(newW, newH)
        .Crop(new SixLabors.ImageSharp.Rectangle((newW - 1200) / 2, (newH - 800) / 2, 1200, 800)));
    await image.SaveAsWebpAsync(outputPath, new SixLabors.ImageSharp.Formats.Webp.WebpEncoder { Quality = 80 });

    return Results.Ok(new { url = $"/uploads/backgrounds/{fileName}" });
}).DisableAntiforgery().RequireRateLimiting("tracking");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
