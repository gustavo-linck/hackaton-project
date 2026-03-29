using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Models;
using UmbLink.Infrastructure.Data.Entities;
using UmbLink.Infrastructure.Repositories;

namespace UmbLink.Application.Services;

public class PlanLimitService(IPlanLimitRepository planLimitRepo) : IPlanLimitService
{
    public async Task<Result<bool>> CanAddPageAsync(Guid userId)
    {
        var limits = await GetLimitsAsync(userId);
        if (limits.MaxPages == -1) return Result<bool>.Ok(true);

        var count = await planLimitRepo.CountPagesByUserAsync(userId, PageStatus.Suspended);

        if (count >= limits.MaxPages)
        {
            var planName = await planLimitRepo.GetPlanNameByUserAsync(userId);
            return Result<bool>.LimitExceeded(new LimitExceededError
            {
                FeatureName = "Páginas",
                Message = $"Você atingiu o limite de {limits.MaxPages} página(s) do plano atual.",
                CurrentPlan = planName,
                RequiredPlan = limits.MaxPages == 1 ? "Pro" : "Business"
            });
        }
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> CanAddLinkAsync(Guid userId, Guid pageId)
    {
        var limits = await GetLimitsAsync(userId);
        if (limits.MaxLinksPerPage == -1) return Result<bool>.Ok(true);

        var count = await planLimitRepo.CountLinksByPageAsync(pageId);

        if (count >= limits.MaxLinksPerPage)
        {
            var planName = await planLimitRepo.GetPlanNameByUserAsync(userId);
            return Result<bool>.LimitExceeded(new LimitExceededError
            {
                FeatureName = "Links",
                Message = $"Limite de {limits.MaxLinksPerPage} links por página atingido.",
                CurrentPlan = planName,
                RequiredPlan = limits.MaxLinksPerPage <= 3 ? "Pro" : "Business"
            });
        }
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> CanUseFeatureAsync(Guid userId, Feature feature)
    {
        var limits = await GetLimitsAsync(userId);
        var planName = await planLimitRepo.GetPlanNameByUserAsync(userId);

        var (allowed, requiredPlan, featureName) = feature switch
        {
            Feature.CustomDomain   => (limits.AllowCustomDomain,   "Pro",      "Domínio customizado"),
            Feature.RemoveBranding => (limits.AllowRemoveBranding, "Business", "Remoção de branding"),
            Feature.Referrer       => (limits.AllowReferrer,       "Pro",      "Origem de acesso"),
            Feature.AdvancedThemes => (limits.ThemeCount != 2,     "Pro",      "Temas avançados"),
            Feature.AdvancedFonts  => (limits.FontCount != 2,      "Pro",      "Fontes avançadas"),
            Feature.AdvancedColors => (limits.ThemeCount != 2,     "Pro",      "Personalização de cores"),
            _ => (false, "Pro", feature.ToString())
        };

        if (!allowed)
            return Result<bool>.LimitExceeded(new LimitExceededError
            {
                FeatureName = featureName,
                Message = $"{featureName} está disponível a partir do plano {requiredPlan}.",
                CurrentPlan = planName,
                RequiredPlan = requiredPlan
            });

        return Result<bool>.Ok(true);
    }

    public async Task<PlanLimitDto> GetLimitsAsync(Guid userId)
    {
        var planId = await planLimitRepo.GetPlanIdByUserAsync(userId);

        if (planId is null)
        {
            var freePlanId = await planLimitRepo.GetFreePlanIdAsync();
            planId = freePlanId ?? 0;
        }

        var limit = await planLimitRepo.GetByPlanIdAsync(planId.Value)
            ?? new PlanLimit { MaxPages = 1, MaxLinksPerPage = 3, AnalyticsDays = 7,
                AllowReferrer = false, AllowCustomDomain = false, AllowRemoveBranding = false,
                ThemeCount = 2, FontCount = 2 };

        return new PlanLimitDto(
            limit.MaxPages,
            limit.MaxLinksPerPage,
            limit.AnalyticsDays,
            limit.AllowReferrer,
            limit.AllowCustomDomain,
            limit.AllowRemoveBranding,
            limit.ThemeCount,
            limit.FontCount
        );
    }
}
