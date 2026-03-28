using FluentValidation;
using UmbLink.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Requests;
using UmbLink.Application.Services;
using UmbLink.Application.BackgroundServices;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Identity;
using UmbLink.Infrastructure.Seed;

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

// Application services
builder.Services.AddScoped<IPlanLimitService, PlanLimitService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<ILinkService, LinkService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IMetricsService, MetricsService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAuditService, AuditService>();

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
    return Results.Redirect("/dashboard");
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
