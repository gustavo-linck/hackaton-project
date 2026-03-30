using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Services;
using UmbLink.Infrastructure.Data.Entities;
using UmbLink.Infrastructure.Identity;
using UmbLink.Infrastructure.Repositories;

namespace UmbLink.UnitTests;

public class AdminServiceTests
{
    private readonly Mock<ISubscriptionRepository> _subRepo = new();
    private readonly Mock<IAnalyticsRepository> _analyticsRepo = new();
    private readonly Mock<IAuditService> _audit = new();
    private readonly Mock<UserManager<AppUser>> _userManager;
    private readonly Mock<IAdminUserRepository> _adminUserRepo = new();
    private readonly Mock<IAuditLogRepository> _auditLogRepo = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly AdminService _sut;

    public AdminServiceTests()
    {
        var store = new Mock<IUserStore<AppUser>>();
        _userManager = new Mock<UserManager<AppUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        _sut = new AdminService(
            _subRepo.Object, _analyticsRepo.Object, _audit.Object,
            _userManager.Object, _adminUserRepo.Object, _auditLogRepo.Object,
            _cache.Object);
    }

    [Fact]
    public async Task GrantTrialAsync_UserNotFound_ReturnsFailure()
    {
        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        var result = await _sut.GrantTrialAsync(Guid.NewGuid(), Guid.NewGuid(), 2, 7);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Usuário não encontrado.");
    }

    [Fact]
    public async Task GrantTrialAsync_AlreadyUsedTrial_ReturnsFailure()
    {
        var targetId = Guid.NewGuid();
        _userManager.Setup(m => m.FindByIdAsync(targetId.ToString()))
            .ReturnsAsync(new AppUser { Id = targetId, Name = "Test", IsActive = true });
        _subRepo.Setup(r => r.HasUsedTrialAsync(targetId, 2)).ReturnsAsync(true);

        var result = await _sut.GrantTrialAsync(Guid.NewGuid(), targetId, 2, 7);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Este usuário já utilizou o trial deste plano.");
    }

    [Fact]
    public async Task GrantTrialAsync_ValidRequest_SetsTrialStatusAndAudits()
    {
        var adminId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var plan = new Plan { Id = 2, Name = "Pro", IsActive = true };
        var sub = new Subscription { UserId = targetId, PlanId = 1 };

        _userManager.Setup(m => m.FindByIdAsync(targetId.ToString()))
            .ReturnsAsync(new AppUser { Id = targetId, Name = "Test", IsActive = true });
        _subRepo.Setup(r => r.HasUsedTrialAsync(targetId, 2)).ReturnsAsync(false);
        _subRepo.Setup(r => r.GetPlanByIdAsync(2)).ReturnsAsync(plan);
        _subRepo.Setup(r => r.GetByUserIdAsync(targetId)).ReturnsAsync(sub);
        _cache.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _audit.Setup(a => a.LogAsync(adminId, "admin.trial_granted", It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.GrantTrialAsync(adminId, targetId, 2, 7);

        result.IsSuccess.Should().BeTrue();
        sub.Status.Should().Be(SubscriptionStatus.Trial);
        sub.PlanId.Should().Be(2);
        sub.TrialEndsAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));
        _subRepo.Verify(r => r.AddTrialUsageAsync(
            It.Is<TrialUsage>(t => t.UserId == targetId && t.PlanId == 2 && t.Status == TrialStatus.Active)),
            Times.Once);
        _subRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _audit.Verify(a => a.LogAsync(adminId, "admin.trial_granted", It.IsAny<object?>()), Times.Once);
    }
}
