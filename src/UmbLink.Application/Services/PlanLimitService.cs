using Microsoft.EntityFrameworkCore;
using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Models;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Application.Services;

public class PlanLimitService(AppDbContext db) : IPlanLimitService
{
    public async Task<Result<bool>> CanAddPageAsync(Guid userId)
    {
        var limits = await GetLimitsAsync(userId);
        if (limits.MaxPages == -1) return Result<bool>.Ok(true);

        var count = await db.Pages.CountAsync(p =>
            p.UserId == userId && p.Status != PageStatus.Suspended);

        if (count >= limits.MaxPages)
        {
            var planName = await GetPlanNameAsync(userId);
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

        var count = await db.Links.CountAsync(l => l.PageId == pageId);

        if (count >= limits.MaxLinksPerPage)
        {
            var planName = await GetPlanNameAsync(userId);
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
        var planName = await GetPlanNameAsync(userId);

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
        var planId = await db.Subscriptions
            .Where(s => s.UserId == userId)
            .Select(s => (int?)s.PlanId)
            .FirstOrDefaultAsync();

        if (planId is null)
        {
            // Usuário sem subscription — retorna limites do Free (MaxPages=1, MaxLinksPerPage=3)
            var freePlanId = await db.Plans
                .Where(p => p.Name == "Free")
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync();
            planId = freePlanId ?? 0;
        }

        var limit = await db.PlanLimits.FirstOrDefaultAsync(pl => pl.PlanId == planId)
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

    private async Task<string> GetPlanNameAsync(Guid userId) =>
        await db.Subscriptions
            .Where(s => s.UserId == userId)
            .Select(s => s.Plan.Name)
            .FirstOrDefaultAsync() ?? "Free";
}
