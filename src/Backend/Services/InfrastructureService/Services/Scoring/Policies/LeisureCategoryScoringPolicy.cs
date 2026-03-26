using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Models;
using InfrastructureService.Services.Scoring.ScoringEntities;

namespace InfrastructureService.Services.Scoring.Policies;

public sealed class LeisureCategoryScoringPolicy : CategoryScoringPolicyBase
{
    private const decimal DailyLeisureMaxScore = 35m;
    private const decimal GreenRecreationMaxScore = 20m;
    private const decimal ActiveLeisureMaxScore = 15m;
    private const decimal CultureMaxScore = 20m;
    private const decimal DiversityBonusMaxScore = 10m;

    private static readonly IReadOnlyDictionary<string, decimal> DailyLeisureWeights = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["cafe"] = 1.00m,
        ["restaurant"] = 1.00m,
        ["coffee_shop"] = 1.00m,
        ["bakery"] = 1.00m,
        ["bar"] = 0.50m
    };

    private static readonly IReadOnlyDictionary<string, decimal> GreenRecreationWeights = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["park"] = 1.00m,
        ["waterfront"] = 0.95m,
        ["recreation_zone"] = 0.80m,
        ["square"] = 0.65m
    };

    private static readonly IReadOnlyDictionary<string, decimal> ActiveLeisureWeights = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["fitness"] = 1.00m,
        ["gym"] = 0.95m,
        ["pool"] = 1.00m,
        ["sports_center"] = 0.90m
    };

    private static readonly IReadOnlyDictionary<string, decimal> CultureWeights = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["cinema"] = 1.00m,
        ["theater"] = 0.95m,
        ["museum"] = 0.80m,
        ["gallery"] = 0.70m,
        ["exhibition_hall"] = 0.70m,
        ["library"] = 0.55m
    };

    private static readonly string[] LeisureBlockOrder = ["daily_leisure", "green_recreation", "active_leisure", "culture"];

    public override string CategoryCode => "leisure";

    public override CategoryScoreBreakdownDto Score(
        Location location,
        IReadOnlyCollection<InfrastructureScoringEntity> entities,
        InfrastructureDetailsDto details,
        Configuration.InfrastructureScoringProfileOptions profile)
    {
        var relevantEntities = entities
            .OfType<LeisureScoringEntity>()
            .Where(static entity => !string.IsNullOrWhiteSpace(entity.SubcategoryCode))
            .ToArray();

        if (relevantEntities.Length == 0)
        {
            return new CategoryScoreBreakdownDto
            {
                MissingImportantSubcategories = LeisureBlockOrder,
                MissingBlocks = LeisureBlockOrder,
                DebugInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["DailyLeisureScore"] = "0",
                    ["DailyLeisureCount"] = "0",
                    ["DailyLeisureNearestDistance"] = "none",
                    ["GreenRecreationScore"] = "0",
                    ["ActiveLeisureScore"] = "0",
                    ["CultureScore"] = "0",
                    ["LeisureDiversityBonus"] = "0",
                    ["AppliedLeisureCap"] = "none",
                    ["PresentLeisureBlocks"] = string.Empty,
                    ["MissingLeisureBlocks"] = string.Join(",", LeisureBlockOrder)
                }
            };
        }

        var bySubcategory = relevantEntities
            .GroupBy(static entity => entity.SubcategoryCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                static group => group.Key,
                group => group.OrderBy(static entity => entity.DistanceMeters).ToArray(),
                StringComparer.OrdinalIgnoreCase);

        var scoreBySubcategory = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var (subcategory, items) in bySubcategory)
        {
            scoreBySubcategory[subcategory] = Round(CalculateSoftProximityScore(items[0].DistanceMeters, 2400m, 10m, 1m, 0.70m));
        }

        var groupedCounts = bySubcategory.ToDictionary(static pair => pair.Key, static pair => pair.Value.Length, StringComparer.OrdinalIgnoreCase);
        var presentSubcategories = bySubcategory.Keys.OrderBy(static value => value, StringComparer.OrdinalIgnoreCase).ToArray();

        var daily = ScoreDailyLeisure(bySubcategory);
        var green = ScoreWeightedBlock(bySubcategory, GreenRecreationWeights, "green_recreation", GreenRecreationMaxScore, 2200m, 15m, 5m, 2m, 2, 0.65m);
        var active = ScoreWeightedBlock(bySubcategory, ActiveLeisureWeights, "active_leisure", ActiveLeisureMaxScore, 2200m, 12m, 3m, 2m, 2, 0.65m);
        var culture = ScoreWeightedBlock(bySubcategory, CultureWeights, "culture", CultureMaxScore, 3200m, 14m, 6m, 2m, 3, 0.55m);

        var blockScores = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["daily_leisure"] = daily.Score,
            ["green_recreation"] = green.Score,
            ["active_leisure"] = active.Score,
            ["culture"] = culture.Score
        };

        var presentBlocks = blockScores
            .Where(static pair => pair.Value > 0m)
            .Select(static pair => pair.Key)
            .OrderBy(static value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var missingBlocks = LeisureBlockOrder
            .Except(presentBlocks, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var diversityBonus = CalculateDiversityBonus(presentBlocks.Length);
        var rawScore = daily.Score + green.Score + active.Score + culture.Score + diversityBonus;
        var completenessAchieved = presentBlocks.Length == LeisureBlockOrder.Length;
        var (score, appliedCap, capReason) = ApplySubcategoryCap(
            rawScore,
            presentBlocks.Length,
            completenessAchieved,
            minimumSubcategoriesForMaxScore: 4,
            singleCap: 45m,
            twoCap: 72m,
            threeCap: 88m);

        var debugInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["DailyLeisureScore"] = daily.Score.ToString("0.##"),
            ["DailyLeisureCount"] = daily.WeightedCount.ToString("0.##"),
            ["DailyLeisureNearestDistance"] = daily.NearestDistanceMeters?.ToString("0.##") ?? "none",
            ["GreenRecreationScore"] = green.Score.ToString("0.##"),
            ["ActiveLeisureScore"] = active.Score.ToString("0.##"),
            ["CultureScore"] = culture.Score.ToString("0.##"),
            ["LeisureDiversityBonus"] = diversityBonus.ToString("0.##"),
            ["AppliedLeisureCap"] = appliedCap?.ToString("0.##") ?? "none",
            ["PresentLeisureBlocks"] = string.Join(",", presentBlocks),
            ["MissingLeisureBlocks"] = string.Join(",", missingBlocks),
            ["daily_leisure_proximity_score"] = daily.ProximityScore.ToString("0.##"),
            ["daily_leisure_count_score"] = daily.CountScore.ToString("0.##"),
            ["green_recreation_proximity_score"] = green.ProximityScore.ToString("0.##"),
            ["green_recreation_count_score"] = green.CountScore.ToString("0.##"),
            ["active_leisure_proximity_score"] = active.ProximityScore.ToString("0.##"),
            ["active_leisure_count_score"] = active.CountScore.ToString("0.##"),
            ["culture_proximity_score"] = culture.ProximityScore.ToString("0.##"),
            ["culture_count_score"] = culture.CountScore.ToString("0.##")
        };

        return new CategoryScoreBreakdownDto
        {
            Score = score,
            ProximityScore = Round(daily.ProximityScore + green.ProximityScore + active.ProximityScore + culture.ProximityScore),
            DensityScore = Round(daily.CountScore + green.CountScore + active.CountScore + culture.CountScore),
            DiversityScore = Round(diversityBonus),
            CompletenessBonus = 0m,
            AppliedCap = appliedCap,
            AppliedCapReason = capReason,
            CompletenessAchieved = completenessAchieved,
            PresentSubcategories = presentSubcategories,
            MissingImportantSubcategories = missingBlocks,
            PresentBlocks = presentBlocks,
            MissingBlocks = missingBlocks,
            ScoreByBlock = blockScores,
            ScoreBySubcategory = scoreBySubcategory,
            GroupedCounts = groupedCounts,
            DebugInfo = debugInfo
        };
    }

    private static BlockScoreResult ScoreDailyLeisure(IReadOnlyDictionary<string, LeisureScoringEntity[]> bySubcategory)
    {
        var relevantEntities = GetEntities(bySubcategory, DailyLeisureWeights);
        if (relevantEntities.Count == 0)
        {
            return BlockScoreResult.Empty("daily_leisure");
        }

        var weightedCount = relevantEntities.Sum(item => item.Weight);
        var nearestDistance = relevantEntities.Min(static item => item.Entity.DistanceMeters);
        var nearestScore = relevantEntities
            .Max(item => CalculateLinearProximityScore(item.Entity.DistanceMeters, 1200m, 12m, item.Weight));
        var countScore = InterpolateSaturation(weightedCount, dailyLeisureCurve) * 23m;
        var score = Math.Min(DailyLeisureMaxScore, nearestScore + countScore);

        return new BlockScoreResult("daily_leisure", Round(score), Round(nearestScore), Round(countScore), Round(weightedCount), Round(nearestDistance));
    }

    private static BlockScoreResult ScoreWeightedBlock(
        IReadOnlyDictionary<string, LeisureScoringEntity[]> bySubcategory,
        IReadOnlyDictionary<string, decimal> weights,
        string blockCode,
        decimal maxScore,
        decimal radiusMeters,
        decimal proximityMaxScore,
        decimal countMaxScore,
        decimal countCap,
        int typeRichnessCap,
        decimal proximityExponent)
    {
        var relevantEntities = GetEntities(bySubcategory, weights);
        if (relevantEntities.Count == 0)
        {
            return BlockScoreResult.Empty(blockCode);
        }

        var proximityScore = relevantEntities
            .Max(item => CalculateSoftProximityScore(item.Entity.DistanceMeters, radiusMeters, proximityMaxScore, item.Weight, proximityExponent));
        var weightedCount = relevantEntities.Sum(item => item.Weight);
        var countContribution = Math.Clamp(weightedCount / Math.Max(countCap, 1m), 0m, 1m) * (countMaxScore * 0.6m);
        var typeRichnessContribution = Math.Clamp(relevantEntities.Select(item => item.Entity.SubcategoryCode).Distinct(StringComparer.OrdinalIgnoreCase).Count() / (decimal)Math.Max(typeRichnessCap, 1), 0m, 1m) * (countMaxScore * 0.4m);
        var countScore = countContribution + typeRichnessContribution;
        var score = Math.Min(maxScore, proximityScore + countScore);

        return new BlockScoreResult(blockCode, Round(score), Round(proximityScore), Round(countScore), Round(weightedCount), relevantEntities.Min(static item => item.Entity.DistanceMeters));
    }

    private static List<WeightedLeisureEntity> GetEntities(
        IReadOnlyDictionary<string, LeisureScoringEntity[]> bySubcategory,
        IReadOnlyDictionary<string, decimal> weights)
    {
        var result = new List<WeightedLeisureEntity>();
        foreach (var (subcategory, weight) in weights)
        {
            if (!bySubcategory.TryGetValue(subcategory, out var items))
            {
                continue;
            }

            result.AddRange(items.Select(item => new WeightedLeisureEntity(item, weight)));
        }

        return result;
    }

    private static decimal CalculateDiversityBonus(int presentBlockCount)
        => presentBlockCount switch
        {
            >= 4 => DiversityBonusMaxScore,
            3 => 8m,
            2 => 4m,
            _ => 0m
        };

    private static decimal CalculateLinearProximityScore(decimal distanceMeters, decimal radiusMeters, decimal maxScore, decimal weight)
        => CalculateProximityContribution(distanceMeters, radiusMeters, maxScore) * weight;

    private static decimal CalculateSoftProximityScore(decimal distanceMeters, decimal radiusMeters, decimal maxScore, decimal weight, decimal exponent)
    {
        if (radiusMeters <= 0m)
        {
            return 0m;
        }

        var linearContribution = Math.Clamp((radiusMeters - distanceMeters) / radiusMeters, 0m, 1m);
        if (linearContribution <= 0m)
        {
            return 0m;
        }

        var softened = (decimal)Math.Pow((double)linearContribution, (double)exponent);
        return softened * maxScore * weight;
    }

    private static decimal InterpolateSaturation(decimal count, IReadOnlyList<SaturationPoint> curve)
    {
        if (curve.Count == 0 || count <= curve[0].Count)
        {
            return curve.Count == 0 ? 0m : curve[0].Score;
        }

        for (var index = 1; index < curve.Count; index += 1)
        {
            var previous = curve[index - 1];
            var current = curve[index];
            if (count > current.Count)
            {
                continue;
            }

            var segmentLength = current.Count - previous.Count;
            if (segmentLength <= 0m)
            {
                return current.Score;
            }

            var progress = (count - previous.Count) / segmentLength;
            return previous.Score + ((current.Score - previous.Score) * progress);
        }

        return curve[^1].Score;
    }

    private static readonly SaturationPoint[] dailyLeisureCurve =
    [
        new(0m, 0m),
        new(1m, 6m / 23m),
        new(2m, 10m / 23m),
        new(3m, 14m / 23m),
        new(4m, 18m / 23m),
        new(5m, 21m / 23m),
        new(6m, 1m)
    ];

    private sealed record WeightedLeisureEntity(LeisureScoringEntity Entity, decimal Weight);

    private sealed record BlockScoreResult(
        string BlockCode,
        decimal Score,
        decimal ProximityScore,
        decimal CountScore,
        decimal WeightedCount,
        decimal? NearestDistanceMeters)
    {
        public static BlockScoreResult Empty(string blockCode) => new(blockCode, 0m, 0m, 0m, 0m, null);
    }

    private sealed record SaturationPoint(decimal Count, decimal Score);
}
