using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Models;
using InfrastructureService.Services.Scoring.Configuration;
using InfrastructureService.Services.Scoring.ScoringEntities;

namespace InfrastructureService.Services.Scoring.Policies;

public abstract class GenericCategoryScoringPolicyBase : CategoryScoringPolicyBase
{
    protected abstract GenericCategoryScoringRulesOptions GetRules(InfrastructureScoringProfileOptions profile);
    protected virtual bool PreferRoutingScore => false;

    public override CategoryScoreBreakdownDto Score(
        Location location,
        IReadOnlyCollection<InfrastructureScoringEntity> entities,
        InfrastructureDetailsDto details,
        InfrastructureScoringProfileOptions profile)
    {
        var rules = GetRules(profile);
        if (PreferRoutingScore && TryScoreWithRouting(details, rules, out var routingScore))
        {
            return routingScore;
        }

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

        var normalizedScore = Math.Min(score, 100m);

        return new CategoryScoreBreakdownDto
        {
            Score = Round(normalizedScore),
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

    private bool TryScoreWithRouting(
        InfrastructureDetailsDto details,
        GenericCategoryScoringRulesOptions rules,
        out CategoryScoreBreakdownDto result)
    {
        var relevantCategories = details.NearbyCategories
            .Where(category => category.CategoryCode.Equals(CategoryCode, StringComparison.OrdinalIgnoreCase))
            .Where(category => rules.Subcategories.ContainsKey(category.SubcategoryCode))
            .ToArray();

        if (relevantCategories.Length == 0)
        {
            result = new CategoryScoreBreakdownDto();
            return false;
        }

        var scoreBySubcategory = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var groupedCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var presentSubcategories = new List<string>();
        var missingImportant = new List<string>();

        var totalWeight = rules.Subcategories.Values.Sum(static x => x.ProximityWeight + x.DensityWeight);
        decimal proximityScore = 0m;
        decimal reachabilityScore = 0m;
        decimal densityScore = 0m;

        foreach (var (subcategory, config) in rules.Subcategories)
        {
            var category = relevantCategories.FirstOrDefault(x => x.SubcategoryCode.Equals(subcategory, StringComparison.OrdinalIgnoreCase));
            if (category is null || (category.CandidateCount == 0 && category.CountInPrimaryRadius == 0 && category.NearestDistanceM is null))
            {
                groupedCounts[subcategory] = 0;
                if (rules.ImportantSubcategories.Contains(subcategory, StringComparer.OrdinalIgnoreCase))
                {
                    missingImportant.Add(subcategory);
                }

                continue;
            }

            presentSubcategories.Add(subcategory);
            groupedCounts[subcategory] = Math.Max(category.CandidateCount, category.CountInPrimaryRadius);
            var weight = totalWeight <= 0 ? 0m : (config.ProximityWeight + config.DensityWeight) / totalWeight;
            var nearestComponent = CalculateRoutingNearestComponent(category, config);
            var reachabilityComponent = CalculateReachabilityComponent(category);
            var densityComponent = CalculateDensityComponent(Math.Max(category.CandidateCount, category.CountInPrimaryRadius));

            proximityScore += nearestComponent * 50m * weight;
            reachabilityScore += reachabilityComponent * 30m * weight;
            densityScore += densityComponent * 20m * weight;
            scoreBySubcategory[subcategory] = Round((nearestComponent * 50m + reachabilityComponent * 30m + densityComponent * 20m) * weight);
        }

        var completenessAchieved = rules.CompletenessRequiredSubcategories.All(presentSubcategories.Contains);
        var completenessBonus = completenessAchieved ? rules.CompletenessBonusWeight : 0m;
        var diversityTotal = Math.Max(rules.DiversitySubcategories.Count + rules.DiversityAnyOfGroups.Count, 1);
        var diversityPresent =
            rules.DiversitySubcategories.Count(presentSubcategories.Contains)
            + rules.DiversityAnyOfGroups.Count(group => group.Any(presentSubcategories.Contains));
        var diversityScore = (diversityPresent / (decimal)diversityTotal) * rules.DiversityWeight;
        var score = Math.Min(proximityScore + reachabilityScore + densityScore + completenessBonus, 100m);

        result = new CategoryScoreBreakdownDto
        {
            Score = Round(score),
            ProximityScore = Round(proximityScore),
            DensityScore = Round(densityScore),
            DiversityScore = Round(diversityScore),
            CompletenessBonus = Round(completenessBonus),
            CompletenessAchieved = completenessAchieved,
            PresentSubcategories = presentSubcategories.OrderBy(static x => x, StringComparer.OrdinalIgnoreCase).ToArray(),
            MissingImportantSubcategories = missingImportant.OrderBy(static x => x, StringComparer.OrdinalIgnoreCase).ToArray(),
            GroupedCounts = groupedCounts,
            ScoreBySubcategory = scoreBySubcategory,
            DebugInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["routing_mode"] = "enabled",
                ["routing_diversity_ratio"] = $"{diversityPresent}/{diversityTotal}"
            }
        };
        return true;
    }

    private static decimal CalculateRoutingNearestComponent(InfrastructureNearbyCategoryDto category, GenericSubcategoryRulesOptions config)
    {
        if (category.WalkingMinutes is decimal walkingMinutes)
        {
            var targetMinutes = Math.Max(config.ProximityRadiusMeters / 80m, 5m);
            return Math.Clamp((targetMinutes - walkingMinutes) / targetMinutes, 0m, 1m);
        }

        var distance = category.RouteDistanceMeters ?? category.NearestDistanceM;
        return distance is null ? 0m : CalculateProximityContribution(distance.Value, config.ProximityRadiusMeters, 1m);
    }

    private static decimal CalculateReachabilityComponent(InfrastructureNearbyCategoryDto category)
    {
        var score = category.ReachableWithin10MinCount + (Math.Max(category.ReachableWithin15MinCount - category.ReachableWithin10MinCount, 0) * 0.6m);
        return Math.Clamp(score / 2.5m, 0m, 1m);
    }

    private static decimal CalculateDensityComponent(int candidateCount)
    {
        if (candidateCount <= 0)
        {
            return 0m;
        }

        return candidateCount switch
        {
            1 => 0.55m,
            2 => 0.82m,
            3 => 1m,
            _ => 1m
        };
    }
}
