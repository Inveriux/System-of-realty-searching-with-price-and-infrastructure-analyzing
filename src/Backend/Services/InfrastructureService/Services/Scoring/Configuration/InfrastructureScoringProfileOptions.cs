namespace InfrastructureService.Services.Scoring.Configuration;

public sealed class InfrastructureScoringProfileOptions
{
    public CategoryWeightOptions TotalWeights { get; set; } = new();
    public CategoryScoringRulesOptions DefaultCategoryRules { get; set; } = new();
    public TransportScoringRulesOptions Transport { get; set; } = new();
    public GenericCategoryScoringRulesOptions Healthcare { get; set; } = GenericCategoryScoringRulesOptions.CreateHealthcareDefaults();
    public GenericCategoryScoringRulesOptions Commerce { get; set; } = GenericCategoryScoringRulesOptions.CreateCommerceDefaults();
    public GenericCategoryScoringRulesOptions Education { get; set; } = GenericCategoryScoringRulesOptions.CreateEducationDefaults();
    public GenericCategoryScoringRulesOptions Leisure { get; set; } = GenericCategoryScoringRulesOptions.CreateLeisureDefaults();
}

public sealed class CategoryWeightOptions
{
    public decimal Education { get; set; } = 1.10m;
    public decimal Healthcare { get; set; } = 1.05m;
    public decimal Transport { get; set; } = 1.20m;
    public decimal Commerce { get; set; } = 0.95m;
    public decimal Leisure { get; set; } = 0.85m;
}

public class CategoryScoringRulesOptions
{
    public decimal ProximityWeight { get; set; } = 35m;
    public decimal DensityWeight { get; set; } = 20m;
    public decimal DiversityWeight { get; set; } = 35m;
    public decimal CompletenessBonusWeight { get; set; } = 10m;
    public decimal SingleSubcategoryCap { get; set; } = 45m;
    public decimal TwoSubcategoryCap { get; set; } = 70m;
    public decimal ThreeSubcategoryCap { get; set; } = 85m;
    public int MinimumSubcategoriesForMaxScore { get; set; } = 4;
}

public sealed class GenericCategoryScoringRulesOptions : CategoryScoringRulesOptions
{
    public Dictionary<string, GenericSubcategoryRulesOptions> Subcategories { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyCollection<string> ImportantSubcategories { get; set; } = [];
    public IReadOnlyCollection<string> MaxScoreRequiredSubcategories { get; set; } = [];
    public IReadOnlyCollection<string> DiversitySubcategories { get; set; } = [];
    public IReadOnlyCollection<IReadOnlyCollection<string>> DiversityAnyOfGroups { get; set; } = [];
    public IReadOnlyCollection<string> CompletenessRequiredSubcategories { get; set; } = [];
    public IReadOnlyCollection<IReadOnlyCollection<string>> CompletenessAnyOfGroups { get; set; } = [];
    public decimal DensityTargetUsefulCount { get; set; } = 6m;
    public decimal ProximityTopEntityWeight { get; set; } = 0.75m;
    public decimal ProximitySecondEntityWeight { get; set; } = 0.25m;

    public static GenericCategoryScoringRulesOptions CreateHealthcareDefaults()
        => new()
        {
            ImportantSubcategories = ["pharmacy", "clinic", "hospital"],
            MaxScoreRequiredSubcategories = ["pharmacy", "clinic"],
            DiversitySubcategories = ["pharmacy", "clinic"],
            DiversityAnyOfGroups = [["hospital", "dentistry", "laboratory"]],
            CompletenessRequiredSubcategories = ["pharmacy", "clinic"],
            CompletenessAnyOfGroups = [["hospital", "dentistry", "laboratory"]],
            DensityTargetUsefulCount = 6m,
            Subcategories = new(StringComparer.OrdinalIgnoreCase)
            {
                ["pharmacy"] = new(1400m, 3, 0.28m, 0.24m),
                ["clinic"] = new(2200m, 2, 0.27m, 0.26m),
                ["hospital"] = new(3200m, 1, 0.23m, 0.20m),
                ["dentistry"] = new(2200m, 2, 0.12m, 0.15m),
                ["laboratory"] = new(2200m, 2, 0.10m, 0.15m)
            }
        };

    public static GenericCategoryScoringRulesOptions CreateCommerceDefaults()
        => new()
        {
            ImportantSubcategories = ["supermarket", "mall", "convenience_store", "drogerie"],
            MaxScoreRequiredSubcategories = ["supermarket"],
            DiversitySubcategories = ["supermarket"],
            DiversityAnyOfGroups = [["mall", "drogerie", "convenience_store"], ["electronics", "building_store"]],
            CompletenessRequiredSubcategories = ["supermarket"],
            CompletenessAnyOfGroups = [["mall", "drogerie", "convenience_store"]],
            DensityTargetUsefulCount = 8m,
            Subcategories = new(StringComparer.OrdinalIgnoreCase)
            {
                ["supermarket"] = new(1200m, 3, 0.26m, 0.24m),
                ["mall"] = new(2500m, 1, 0.22m, 0.18m),
                ["convenience_store"] = new(900m, 3, 0.16m, 0.18m),
                ["electronics"] = new(2500m, 1, 0.12m, 0.14m),
                ["building_store"] = new(2500m, 1, 0.12m, 0.14m),
                ["drogerie"] = new(1500m, 2, 0.12m, 0.12m)
            }
        };

    public static GenericCategoryScoringRulesOptions CreateEducationDefaults()
        => new()
        {
            ImportantSubcategories = ["kindergarten", "school"],
            MaxScoreRequiredSubcategories = ["kindergarten", "school"],
            DiversitySubcategories = ["kindergarten", "school"],
            CompletenessRequiredSubcategories = ["kindergarten", "school"],
            DensityTargetUsefulCount = 5m,
            Subcategories = new(StringComparer.OrdinalIgnoreCase)
            {
                ["kindergarten"] = new(2200m, 2, 0.30m, 0.28m),
                ["school"] = new(2200m, 2, 0.34m, 0.30m),
                ["college"] = new(3000m, 1, 0.14m, 0.16m),
                ["university"] = new(3500m, 1, 0.12m, 0.12m)
            }
        };

    public static GenericCategoryScoringRulesOptions CreateLeisureDefaults()
        => new()
        {
            ImportantSubcategories = ["park", "fitness", "museum", "theater", "cinema"],
            MaxScoreRequiredSubcategories = ["park", "fitness"],
            DiversitySubcategories = ["park", "fitness"],
            DiversityAnyOfGroups = [["museum", "theater", "cinema"], ["cafe", "restaurant"]],
            CompletenessRequiredSubcategories = ["park", "fitness"],
            CompletenessAnyOfGroups = [["museum", "theater", "cinema"]],
            DensityTargetUsefulCount = 7m,
            Subcategories = new(StringComparer.OrdinalIgnoreCase)
            {
                ["park"] = new(1800m, 2, 0.26m, 0.22m),
                ["fitness"] = new(2000m, 2, 0.22m, 0.18m),
                ["museum"] = new(3200m, 1, 0.12m, 0.14m),
                ["theater"] = new(3200m, 1, 0.10m, 0.12m),
                ["cinema"] = new(3200m, 1, 0.10m, 0.12m),
                ["cafe"] = new(1500m, 3, 0.10m, 0.12m),
                ["restaurant"] = new(1800m, 3, 0.10m, 0.10m)
            }
        };
}

public sealed class GenericSubcategoryRulesOptions
{
    public GenericSubcategoryRulesOptions()
    {
    }

    public GenericSubcategoryRulesOptions(decimal proximityRadiusMeters, int usefulCountCap, decimal proximityWeight, decimal densityWeight)
    {
        ProximityRadiusMeters = proximityRadiusMeters;
        UsefulCountCap = usefulCountCap;
        ProximityWeight = proximityWeight;
        DensityWeight = densityWeight;
    }

    public decimal ProximityRadiusMeters { get; set; }
    public int UsefulCountCap { get; set; }
    public decimal ProximityWeight { get; set; }
    public decimal DensityWeight { get; set; }
}

public sealed class TransportScoringRulesOptions : CategoryScoringRulesOptions
{
    public decimal SurfaceStopsWeight { get; set; } = 42m;
    public decimal MetroWeight { get; set; } = 15m;
    public decimal RailwayWeight { get; set; } = 10m;
    public decimal ParkingWeight { get; set; } = 13m;
    public decimal DiversityWeightTransport { get; set; } = 15m;
    public decimal CompletenessWeightTransport { get; set; } = 5m;
    public decimal SurfaceNearestWeight { get; set; } = 0.45m;
    public decimal SurfaceCountSaturationWeight { get; set; } = 0.40m;
    public decimal SurfaceModeRichnessWeight { get; set; } = 0.15m;
    public int BusStopGroupUsefulCountCap { get; set; } = 3;
    public int TramStopGroupUsefulCountCap { get; set; } = 2;
    public int TrolleybusStopGroupUsefulCountCap { get; set; } = 2;
    public int MetroUsefulCountCap { get; set; } = 1;
    public int RailwayUsefulCountCap { get; set; } = 1;
    public int ParkingUsefulCountCap { get; set; } = 3;
    public decimal SurfaceStopRadiusMeters { get; set; } = 900m;
    public decimal MetroRadiusMeters { get; set; } = 2800m;
    public decimal RailwayRadiusMeters { get; set; } = 2200m;
    public decimal ParkingRadiusMeters { get; set; } = 1200m;
    public decimal CompletenessMaxDistanceMeters { get; set; } = 1500m;
    public decimal SurfaceOnlyCap { get; set; } = 68m;
    public decimal SurfacePlusParkingCap { get; set; } = 82m;
    public decimal SurfacePlusParkingAndRailwayCap { get; set; } = 92m;
    public decimal SurfacePlusParkingAndMetroCap { get; set; } = 100m;
    public decimal NonMetroMaxCapWithSurfaceAndParking { get; set; } = 95m;
}
