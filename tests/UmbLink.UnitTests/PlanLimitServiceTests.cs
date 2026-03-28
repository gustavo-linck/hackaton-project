using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UmbLink.Application.Models;
using UmbLink.Application.Services;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;
using UmbLink.Infrastructure.Identity;

namespace UmbLink.UnitTests;

public class PlanLimitServiceTests
{
    private static AppDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(opts);
    }

    private static (AppUser user, Plan plan, Subscription sub) SeedUser(
        AppDbContext db, int maxPages, int maxLinksPerPage,
        bool allowCustomDomain = false, bool allowReferrer = false,
        bool allowRemoveBranding = false, int themeCount = 2)
    {
        var plan = new Plan { Name = "TestPlan" };
        db.Plans.Add(plan);
        db.SaveChanges();

        db.PlanLimits.Add(new PlanLimit
        {
            PlanId = plan.Id,
            MaxPages = maxPages,
            MaxLinksPerPage = maxLinksPerPage,
            AnalyticsDays = 7,
            AllowCustomDomain = allowCustomDomain,
            AllowReferrer = allowReferrer,
            AllowRemoveBranding = allowRemoveBranding,
            ThemeCount = themeCount,
            FontCount = 2
        });

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "test@test.com",
            Email = "test@test.com",
            Name = "Test User"
        };
        db.Users.Add(user);
        db.SaveChanges();

        var sub = new Subscription
        {
            UserId = user.Id,
            PlanId = plan.Id,
            Status = SubscriptionStatus.Free
        };
        db.Subscriptions.Add(sub);
        db.SaveChanges();

        return (user, plan, sub);
    }

    // --- CanAddPageAsync ---

    [Fact]
    public async Task CanAddPage_WhenLimitNotReached_ReturnsOk()
    {
        var db = CreateDb();
        var (user, _, _) = SeedUser(db, maxPages: 1, maxLinksPerPage: 3);
        // No pages yet
        var svc = new PlanLimitService(db);

        var result = await svc.CanAddPageAsync(user.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CanAddPage_WhenLimitReached_ReturnsLimitExceeded()
    {
        var db = CreateDb();
        var (user, _, _) = SeedUser(db, maxPages: 1, maxLinksPerPage: 3);
        // Add 1 page (at limit)
        db.Pages.Add(new Page { UserId = user.Id, Slug = "test", Title = "Test",
            Status = PageStatus.Published });
        db.SaveChanges();
        var svc = new PlanLimitService(db);

        var result = await svc.CanAddPageAsync(user.Id);

        result.IsFailure.Should().BeTrue();
        result.LimitError.Should().NotBeNull();
        result.LimitError!.FeatureName.Should().Be("Páginas");
    }

    [Fact]
    public async Task CanAddPage_WhenUnlimited_AlwaysReturnsOk()
    {
        var db = CreateDb();
        var (user, _, _) = SeedUser(db, maxPages: -1, maxLinksPerPage: -1);
        // Add many pages
        for (int i = 0; i < 10; i++)
            db.Pages.Add(new Page { UserId = user.Id, Slug = $"slug-{i}", Title = $"T{i}",
                Status = PageStatus.Published });
        db.SaveChanges();
        var svc = new PlanLimitService(db);

        var result = await svc.CanAddPageAsync(user.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CanAddPage_SuspendedPagesNotCountedAgainstLimit()
    {
        var db = CreateDb();
        var (user, _, _) = SeedUser(db, maxPages: 1, maxLinksPerPage: 3);
        // 1 active + 1 suspended — should still be at limit
        db.Pages.Add(new Page { UserId = user.Id, Slug = "active", Title = "A",
            Status = PageStatus.Published });
        db.Pages.Add(new Page { UserId = user.Id, Slug = "suspended", Title = "S",
            Status = PageStatus.Suspended });
        db.SaveChanges();
        var svc = new PlanLimitService(db);

        var result = await svc.CanAddPageAsync(user.Id);

        result.IsFailure.Should().BeTrue();
    }

    // --- CanAddLinkAsync ---

    [Fact]
    public async Task CanAddLink_WhenLimitNotReached_ReturnsOk()
    {
        var db = CreateDb();
        var (user, _, _) = SeedUser(db, maxPages: 1, maxLinksPerPage: 3);
        var page = new Page { UserId = user.Id, Slug = "test", Title = "T",
            Status = PageStatus.Published };
        db.Pages.Add(page);
        db.SaveChanges();
        db.Links.Add(new Link { PageId = page.Id, Title = "L1", Url = "https://a.com", Order = 1 });
        db.Links.Add(new Link { PageId = page.Id, Title = "L2", Url = "https://b.com", Order = 2 });
        db.SaveChanges(); // 2 links, limit is 3
        var svc = new PlanLimitService(db);

        var result = await svc.CanAddLinkAsync(user.Id, page.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CanAddLink_WhenAtLimit_ReturnsLimitExceeded()
    {
        var db = CreateDb();
        var (user, _, _) = SeedUser(db, maxPages: 1, maxLinksPerPage: 3);
        var page = new Page { UserId = user.Id, Slug = "test", Title = "T",
            Status = PageStatus.Published };
        db.Pages.Add(page);
        db.SaveChanges();
        for (int i = 1; i <= 3; i++)
            db.Links.Add(new Link { PageId = page.Id, Title = $"L{i}",
                Url = "https://a.com", Order = i });
        db.SaveChanges();
        var svc = new PlanLimitService(db);

        var result = await svc.CanAddLinkAsync(user.Id, page.Id);

        result.IsFailure.Should().BeTrue();
        result.LimitError!.FeatureName.Should().Be("Links");
    }

    // --- CanUseFeatureAsync ---

    [Fact]
    public async Task CanUseFeature_CustomDomain_WhenNotAllowed_ReturnsLimitExceeded()
    {
        var db = CreateDb();
        var (user, _, _) = SeedUser(db, maxPages: 1, maxLinksPerPage: 3, allowCustomDomain: false);
        var svc = new PlanLimitService(db);

        var result = await svc.CanUseFeatureAsync(user.Id, Feature.CustomDomain);

        result.IsFailure.Should().BeTrue();
        result.LimitError!.RequiredPlan.Should().Be("Pro");
    }

    [Fact]
    public async Task CanUseFeature_CustomDomain_WhenAllowed_ReturnsOk()
    {
        var db = CreateDb();
        var (user, _, _) = SeedUser(db, maxPages: 3, maxLinksPerPage: 5, allowCustomDomain: true);
        var svc = new PlanLimitService(db);

        var result = await svc.CanUseFeatureAsync(user.Id, Feature.CustomDomain);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CanUseFeature_RemoveBranding_WhenNotAllowed_ReturnsLimitExceeded()
    {
        var db = CreateDb();
        var (user, _, _) = SeedUser(db, maxPages: 3, maxLinksPerPage: 5,
            allowRemoveBranding: false);
        var svc = new PlanLimitService(db);

        var result = await svc.CanUseFeatureAsync(user.Id, Feature.RemoveBranding);

        result.IsFailure.Should().BeTrue();
        result.LimitError!.RequiredPlan.Should().Be("Business");
    }

    // --- GetLimitsAsync ---

    [Fact]
    public async Task GetLimitsAsync_ReturnsCorrectLimits()
    {
        var db = CreateDb();
        var (user, _, _) = SeedUser(db, maxPages: 3, maxLinksPerPage: 5,
            allowCustomDomain: true, themeCount: -1);
        var svc = new PlanLimitService(db);

        var limits = await svc.GetLimitsAsync(user.Id);

        limits.MaxPages.Should().Be(3);
        limits.MaxLinksPerPage.Should().Be(5);
        limits.AllowCustomDomain.Should().BeTrue();
        limits.ThemeCount.Should().Be(-1);
    }
}
