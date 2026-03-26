using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Models;
using InfrastructureService.Services.Scoring.Abstractions;
using InfrastructureService.Services.Scoring.ScoringEntities;

namespace InfrastructureService.Services.Scoring.Policies;

public abstract class CategoryScoringPolicyBase : IInfrastructureCategoryScoringPolicy
{
    public abstract string CategoryCode { get; }

    public abstract CategoryScoreBreakdownDto Score(
        Location location,
        IReadOnlyCollection<InfrastructureScoringEntity> entities,
        InfrastructureDetailsDto details,
        Configuration.InfrastructureScoringProfileOptions profile);

    protected static decimal CalculateProximityContribution(decimal distanceMeters, decimal radiusMeters, decimal maxScore)
        => radiusMeters <= 0m
            ? 0m
            : Math.Clamp((radiusMeters - distanceMeters) / radiusMeters, 0m, 1m) * maxScore;

    protected static decimal CalculateDensityContribution(decimal usefulCount, decimal targetCount, decimal maxScore)
        => targetCount <= 0m
            ? 0m
            : Math.Clamp(usefulCount / targetCount, 0m, 1m) * maxScore;

    protected static decimal Round(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    protected static (decimal Score, decimal? AppliedCap, string? Reason) ApplySubcategoryCap(
        decimal rawScore,
        int presentSubcategoryCount,
        bool completenessAchieved,
        int minimumSubcategoriesForMaxScore,
        decimal singleCap,
        decimal twoCap,
        decimal threeCap)
    {
        decimal? cap = null;
        string? reason = null;
        if (presentSubcategoryCount <= 1)
        {
            cap = singleCap;
            reason = "single_subcategory";
        }
        else if (presentSubcategoryCount == 2)
        {
            cap = twoCap;
            reason = "two_subcategories";
        }
        else if (presentSubcategoryCount == 3 && (!completenessAchieved || minimumSubcategoriesForMaxScore > 3))
        {
            cap = threeCap;
            reason = "three_subcategories_without_full_completeness";
        }

        if (!cap.HasValue)
            return (Round(rawScore), null, null);

        return (Round(Math.Min(rawScore, cap.Value)), cap, reason);
    }
}
