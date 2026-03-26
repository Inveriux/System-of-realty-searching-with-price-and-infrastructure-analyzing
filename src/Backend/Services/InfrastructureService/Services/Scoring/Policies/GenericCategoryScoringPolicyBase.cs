using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Models;
using InfrastructureService.Services.Scoring.Configuration;
using InfrastructureService.Services.Scoring.ScoringEntities;

namespace InfrastructureService.Services.Scoring.Policies;

public abstract class GenericCategoryScoringPolicyBase : CategoryScoringPolicyBase
{
    protected abstract GenericCategoryScoringRulesOptions GetRules(InfrastructureScoringProfileOptions profile);

    public override CategoryScoreBreakdownDto Score(
        Location location,
        IReadOnlyCollection<InfrastructureScoringEntity> entities,
        InfrastructureDetailsDto details,
        InfrastructureScoringProfileOptions profile)
    {
        var rules = GetRules(profile);
        var relevantEntities = entities
            .Where(entity => rules.Subcategories.ContainsKey(entity.SubcategoryCode))
            .ToArray();

        if (relevantEntities.Length == 0)
        {
            return new CategoryScoreBreakdownDto
            {
                MissingImportantSubcategories = rules.ImportantSubcategories.OrderBy(static value => value).ToArray()
            };
        }

        var bySubcategory = relevantEntities
            .GroupBy(static entity => entity.SubcategoryCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                static group => group.Key,
                group => group.OrderBy(static entity => entity.DistanceMeters).ToArray(),
                StringComparer.OrdinalIgnoreCase);

        var presentSubcategories = bySubcategory.Keys.OrderBy(static value => value, StringComparer.OrdinalIgnoreCase).ToArray();
        var missingImportant = rules.ImportantSubcategories
            .Where(subcategory => !bySubcategory.ContainsKey(subcategory))
            .OrderBy(static value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var scoreBySubcategory = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var groupedCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        decimal proximityScore = 0m;
        decimal densityScore = 0m;

        foreach (var (subcategory, config) in rules.Subcategories)
        {
            bySubcategory.TryGetValue(subcategory, out var subcategoryEntities);
            var entityList = subcategoryEntities ?? [];
            var top1 = entityList.ElementAtOrDefault(0);
            var top2 = entityList.ElementAtOrDefault(1);
            var baseProximity =
                (top1 is null ? 0m : CalculateProximityContribution(top1.DistanceMeters, config.ProximityRadiusMeters, 1m) * rules.ProximityTopEntityWeight) +
                (top2 is null ? 0m : CalculateProximityContribution(top2.DistanceMeters, config.ProximityRadiusMeters, 1m) * rules.ProximitySecondEntityWeight);
            var subcategoryProximityScore = baseProximity * rules.ProximityWeight * config.ProximityWeight;
            proximityScore += subcategoryProximityScore;

            var usefulCount = Math.Min(entityList.Length, config.UsefulCountCap);
            densityScore += Math.Clamp(usefulCount / (decimal)Math.Max(config.UsefulCountCap, 1), 0m, 1m) * rules.DensityWeight * config.DensityWeight;
            groupedCounts[subcategory] = entityList.Length;
            scoreBySubcategory[subcategory] = Round(subcategoryProximityScore);
        }

        var diversityPresentCount =
            rules.DiversitySubcategories.Count(bySubcategory.ContainsKey)
            + rules.DiversityAnyOfGroups.Count(group => group.Any(bySubcategory.ContainsKey));
        var diversityTotal = Math.Max(rules.DiversitySubcategories.Count + rules.DiversityAnyOfGroups.Count, 1);
        var diversityScore = (diversityPresentCount / (decimal)diversityTotal) * rules.DiversityWeight;

        var completenessAchieved =
            rules.CompletenessRequiredSubcategories.All(bySubcategory.ContainsKey)
            && rules.CompletenessAnyOfGroups.All(group => group.Count == 0 || group.Any(bySubcategory.ContainsKey));
        var completenessBonus = completenessAchieved ? rules.CompletenessBonusWeight : 0m;

        var rawScore = proximityScore + densityScore + diversityScore + completenessBonus;
        var presentMaxScoreRequiredCount = rules.MaxScoreRequiredSubcategories.Count(bySubcategory.ContainsKey);
        var effectivePresentSubcategoryCount = Math.Max(presentSubcategories.Length, presentMaxScoreRequiredCount);
        var (score, appliedCap, capReason) = ApplySubcategoryCap(
            rawScore,
            effectivePresentSubcategoryCount,
            completenessAchieved && rules.MaxScoreRequiredSubcategories.All(bySubcategory.ContainsKey),
            rules.MinimumSubcategoriesForMaxScore,
            rules.SingleSubcategoryCap,
            rules.TwoSubcategoryCap,
            rules.ThreeSubcategoryCap);

        return new CategoryScoreBreakdownDto
        {
            Score = score,
            ProximityScore = Round(proximityScore),
            DensityScore = Round(densityScore),
            DiversityScore = Round(diversityScore),
            CompletenessBonus = Round(completenessBonus),
            AppliedCap = appliedCap,
            AppliedCapReason = capReason,
            CompletenessAchieved = completenessAchieved,
            PresentSubcategories = presentSubcategories,
            MissingImportantSubcategories = missingImportant,
            GroupedCounts = groupedCounts,
            ScoreBySubcategory = scoreBySubcategory,
            DebugInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["present_subcategories_count"] = presentSubcategories.Length.ToString(),
                ["diversity_ratio"] = $"{diversityPresentCount}/{diversityTotal}",
                ["completeness_bonus"] = completenessAchieved ? "yes" : "no"
            }
        };
    }
}
