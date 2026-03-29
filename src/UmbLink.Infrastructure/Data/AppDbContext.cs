using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UmbLink.Infrastructure.Data.Entities;
using UmbLink.Infrastructure.Identity;
using Link = UmbLink.Infrastructure.Data.Entities.Link;

namespace UmbLink.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<PlanLimit> PlanLimits => Set<PlanLimit>();
    public DbSet<PlanPrice> PlanPrices => Set<PlanPrice>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<TrialUsage> TrialUsages => Set<TrialUsage>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<Link> Links => Set<Link>();
    public DbSet<ClickEvent> ClickEvents => Set<ClickEvent>();
    public DbSet<PageView> PageViews => Set<PageView>();
    public DbSet<CustomDomain> CustomDomains => Set<CustomDomain>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UserActivityLog> UserActivityLogs => Set<UserActivityLog>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // User → Subscription (1:1)
        builder.Entity<AppUser>()
            .HasOne(u => u.Subscription)
            .WithOne(s => s.User)
            .HasForeignKey<Subscription>(s => s.UserId);

        // Plan → PlanLimit (1:1)
        builder.Entity<PlanLimit>()
            .HasOne(pl => pl.Plan)
            .WithOne(p => p.Limit)
            .HasForeignKey<PlanLimit>(pl => pl.PlanId);

        // TrialUsage: unique constraint on (UserId, PlanId)
        builder.Entity<TrialUsage>()
            .HasIndex(t => new { t.UserId, t.PlanId })
            .IsUnique();

        // Page: unique slug
        builder.Entity<Page>()
            .HasIndex(p => p.Slug)
            .IsUnique();

        // Enum → string conversions
        builder.Entity<AppUser>()
            .Property(u => u.Role).HasConversion<string>();
        builder.Entity<Subscription>()
            .Property(s => s.Status).HasConversion<string>();
        builder.Entity<Subscription>()
            .Property(s => s.BillingPeriod).HasConversion<string>();
        builder.Entity<PlanPrice>()
            .Property(pp => pp.BillingPeriod).HasConversion<string>();
        builder.Entity<Page>()
            .Property(p => p.Status).HasConversion<string>();
        builder.Entity<TrialUsage>()
            .Property(t => t.Status).HasConversion<string>();
        builder.Entity<CustomDomain>()
            .Property(cd => cd.Status).HasConversion<string>();

        // CustomDomain: unique index on Domain
        builder.Entity<CustomDomain>()
            .HasIndex(cd => cd.Domain)
            .IsUnique();

        // Decimal precision for PlanPrice
        builder.Entity<PlanPrice>()
            .Property(p => p.PricePerMonth)
            .HasPrecision(10, 2);
        builder.Entity<PlanPrice>()
            .Property(p => p.TotalCharged)
            .HasPrecision(10, 2);

        // Indexes for common query patterns
        builder.Entity<ClickEvent>()
            .HasIndex(c => new { c.LinkId, c.Timestamp });
        builder.Entity<PageView>()
            .HasIndex(v => new { v.PageId, v.Timestamp });
        builder.Entity<Page>()
            .HasIndex(p => p.UserId);

        builder.Entity<UserActivityLog>()
            .HasIndex(l => new { l.UserId, l.CreatedAt });

        builder.Entity<AppUser>()
            .HasOne(u => u.PaymentMethod)
            .WithOne(p => p.User)
            .HasForeignKey<PaymentMethod>(p => p.UserId);
    }
}
