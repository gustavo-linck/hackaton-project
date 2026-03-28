using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;
using UmbLink.Infrastructure.Identity;

namespace UmbLink.Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var userMgr = services.GetRequiredService<UserManager<AppUser>>();
        var roleMgr = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (db.Plans.Any()) return; // idempotente

        // === ROLES ===
        if (!await roleMgr.RoleExistsAsync("Admin"))
            await roleMgr.CreateAsync(new IdentityRole<Guid> { Name = "Admin", NormalizedName = "ADMIN" });
        if (!await roleMgr.RoleExistsAsync("User"))
            await roleMgr.CreateAsync(new IdentityRole<Guid> { Name = "User", NormalizedName = "USER" });

        // === PLANOS ===
        var free = new Plan { Name = "Free" };
        var pro  = new Plan { Name = "Pro" };
        var biz  = new Plan { Name = "Business" };
        db.Plans.AddRange(free, pro, biz);
        await db.SaveChangesAsync();

        // Limites
        db.PlanLimits.AddRange(
            new PlanLimit { PlanId = free.Id, MaxPages = 1, MaxLinksPerPage = 3,
                AnalyticsDays = 7, AllowReferrer = false, AllowCustomDomain = false,
                AllowRemoveBranding = false, ThemeCount = 2, FontCount = 2 },
            new PlanLimit { PlanId = pro.Id, MaxPages = 3, MaxLinksPerPage = 5,
                AnalyticsDays = 365, AllowReferrer = true, AllowCustomDomain = true,
                AllowRemoveBranding = false, ThemeCount = -1, FontCount = -1 },
            new PlanLimit { PlanId = biz.Id, MaxPages = -1, MaxLinksPerPage = -1,
                AnalyticsDays = -1, AllowReferrer = true, AllowCustomDomain = true,
                AllowRemoveBranding = true, ThemeCount = -1, FontCount = -1 }
        );

        // Preços Pro
        db.PlanPrices.AddRange(
            new PlanPrice { PlanId = pro.Id, BillingPeriod = BillingPeriod.Monthly,
                PricePerMonth = 19m, TotalCharged = 19m, DiscountPercent = 0 },
            new PlanPrice { PlanId = pro.Id, BillingPeriod = BillingPeriod.Quarterly,
                PricePerMonth = 16m, TotalCharged = 48m, DiscountPercent = 15 },
            new PlanPrice { PlanId = pro.Id, BillingPeriod = BillingPeriod.Annual,
                PricePerMonth = 14m, TotalCharged = 168m, DiscountPercent = 25 },
            // Preços Business
            new PlanPrice { PlanId = biz.Id, BillingPeriod = BillingPeriod.Monthly,
                PricePerMonth = 75m, TotalCharged = 75m, DiscountPercent = 0 },
            new PlanPrice { PlanId = biz.Id, BillingPeriod = BillingPeriod.Quarterly,
                PricePerMonth = 64m, TotalCharged = 192m, DiscountPercent = 15 },
            new PlanPrice { PlanId = biz.Id, BillingPeriod = BillingPeriod.Annual,
                PricePerMonth = 56m, TotalCharged = 672m, DiscountPercent = 25 }
        );
        await db.SaveChangesAsync();

        // === USUÁRIOS ===
        var admin  = new AppUser { Name = "Admin Umbler",   Email = "admin@umblink.com",  UserName = "admin@umblink.com",  Role = UserRole.Admin };
        var joao   = new AppUser { Name = "João Silva",     Email = "joao@exemplo.com",   UserName = "joao@exemplo.com" };
        var maria  = new AppUser { Name = "Maria Criativa", Email = "maria@exemplo.com",  UserName = "maria@exemplo.com" };
        var carlos = new AppUser { Name = "Carlos Dev",     Email = "carlos@exemplo.com", UserName = "carlos@exemplo.com" };

        await userMgr.CreateAsync(admin,  "Demo@1234");
        await userMgr.CreateAsync(joao,   "Demo@1234");
        await userMgr.CreateAsync(maria,  "Demo@1234");
        await userMgr.CreateAsync(carlos, "Demo@1234");

        await userMgr.AddToRoleAsync(admin, "Admin");
        await userMgr.AddToRoleAsync(joao,  "User");
        await userMgr.AddToRoleAsync(maria, "User");
        await userMgr.AddToRoleAsync(carlos,"User");

        // Subscriptions
        var bizPrice = await db.PlanPrices.FirstAsync(p =>
            p.PlanId == biz.Id && p.BillingPeriod == BillingPeriod.Monthly);

        db.Subscriptions.AddRange(
            new Subscription { UserId = admin.Id,  PlanId = free.Id,
                Status = SubscriptionStatus.Free },
            new Subscription { UserId = joao.Id,   PlanId = biz.Id,
                Status = SubscriptionStatus.Active, BillingPeriod = BillingPeriod.Monthly,
                PlanPriceId = bizPrice.Id,
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1) },
            new Subscription { UserId = maria.Id,  PlanId = pro.Id,
                Status = SubscriptionStatus.Trial,
                TrialStartedAt = DateTime.UtcNow.AddDays(-3),
                TrialEndsAt = DateTime.UtcNow.AddDays(4) },
            new Subscription { UserId = carlos.Id, PlanId = free.Id,
                Status = SubscriptionStatus.Free }
        );
        await db.SaveChangesAsync();

        // === PÁGINAS ===
        var pageJoao = new Page
        {
            UserId = joao.Id, Slug = "joao-silva", Title = "João Silva",
            Bio = "Designer & Fotógrafo 📸", Status = PageStatus.Published,
            ThemeConfig = """{"themeId":"glass-morphism","bgColor":"#1a1a2e","buttonStyle":"rounded","buttonColor":"#e94560","buttonTextColor":"#ffffff","textColor":"#ffffff","titleFont":"poppins","linkFont":"inter","spacing":"normal","avatarShape":"circle"}"""
        };
        var pageMaria = new Page
        {
            UserId = maria.Id, Slug = "maria-criativa", Title = "Maria Criativa",
            Bio = "Artista Visual • UX Designer ✨", Status = PageStatus.Published,
            ThemeConfig = """{"themeId":"gradient-sunset","bgColor":"#ffecd2","buttonStyle":"rounded","buttonColor":"#f97316","buttonTextColor":"#ffffff","textColor":"#1f2937","titleFont":"inter","linkFont":"inter","spacing":"normal","avatarShape":"circle"}"""
        };
        var pageCarlos = new Page
        {
            UserId = carlos.Id, Slug = "carlos-dev", Title = "Carlos Dev",
            Bio = "Desenvolvedor Full Stack 💻", Status = PageStatus.Published,
            ThemeConfig = """{"themeId":"minimal-light","bgColor":"#ffffff","buttonStyle":"outline","buttonColor":"#111111","buttonTextColor":"#111111","textColor":"#111111","titleFont":"inter","linkFont":"inter","spacing":"normal","avatarShape":"circle"}"""
        };
        db.Pages.AddRange(pageJoao, pageMaria, pageCarlos);
        await db.SaveChangesAsync();

        // === LINKS ===
        db.Links.AddRange(
            new Link { PageId = pageJoao.Id,   Title = "Portfolio",  Url = "https://joaosilva.com",              IconName = "globe",        Order = 1, IsActive = true },
            new Link { PageId = pageJoao.Id,   Title = "Instagram",  Url = "https://instagram.com/joaosilva",    IconName = "instagram",    Order = 2, IsActive = true },
            new Link { PageId = pageJoao.Id,   Title = "LinkedIn",   Url = "https://linkedin.com/in/joaosilva",  IconName = "linkedin",     Order = 3, IsActive = true },
            new Link { PageId = pageJoao.Id,   Title = "Behance",    Url = "https://behance.net/joaosilva",      IconName = "behance",      Order = 4, IsActive = true },
            new Link { PageId = pageMaria.Id,  Title = "Instagram",  Url = "https://instagram.com/mariacriativa",IconName = "instagram",    Order = 1, IsActive = true },
            new Link { PageId = pageMaria.Id,  Title = "TikTok",     Url = "https://tiktok.com/@mariacriativa",  IconName = "tiktok",       Order = 2, IsActive = true },
            new Link { PageId = pageMaria.Id,  Title = "Loja Online",Url = "https://loja.mariacriativa.com",     IconName = "shopping-bag", Order = 3, IsActive = true },
            new Link { PageId = pageCarlos.Id, Title = "GitHub",     Url = "https://github.com/carlosdev",       IconName = "github",       Order = 1, IsActive = true },
            new Link { PageId = pageCarlos.Id, Title = "LinkedIn",   Url = "https://linkedin.com/in/carlosdev",  IconName = "linkedin",     Order = 2, IsActive = true },
            new Link { PageId = pageCarlos.Id, Title = "Blog",       Url = "https://carlosdev.com.br",           IconName = "globe",        Order = 3, IsActive = true }
        );
        await db.SaveChangesAsync();

        // === MÉTRICAS SIMULADAS (30 dias) ===
        var rng = new Random(42);
        var allPages = new[] { pageJoao, pageMaria, pageCarlos };
        var allLinks = await db.Links.ToListAsync();

        var views = new List<PageView>();
        var clicks = new List<ClickEvent>();

        for (int d = 29; d >= 0; d--)
        {
            var date = DateTime.UtcNow.Date.AddDays(-d);
            foreach (var page in allPages)
            {
                int count = rng.Next(5, 80);
                for (int i = 0; i < count; i++)
                    views.Add(new PageView
                    {
                        PageId = page.Id,
                        Timestamp = date.AddHours(rng.Next(0, 23)).AddMinutes(rng.Next(0, 59))
                    });
            }
            foreach (var link in allLinks)
            {
                int count = rng.Next(0, 20);
                for (int i = 0; i < count; i++)
                    clicks.Add(new ClickEvent
                    {
                        LinkId = link.Id,
                        Timestamp = date.AddHours(rng.Next(0, 23)).AddMinutes(rng.Next(0, 59))
                    });
            }
        }

        // Inserir em batches para evitar timeout
        db.PageViews.AddRange(views);
        db.ClickEvents.AddRange(clicks);
        await db.SaveChangesAsync();
    }
}
