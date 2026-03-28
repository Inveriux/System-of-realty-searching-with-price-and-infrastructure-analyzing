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
            DensityTargetUsefulCount = 8m,
            Subcategories = new(StringComparer.OrdinalIgnoreCase)
            {
                ["pharmacy"] = new(1000m, 6, 0.31m, 0.31m),
                ["clinic"] = new(1500m, 2, 0.24m, 0.20m),
                ["hospital"] = new(5000m, 1, 0.25m, 0.12m),
                ["dentistry"] = new(1500m, 2, 0.11m, 0.12m),
                ["laboratory"] = new(1500m, 2, 0.09m, 0.10m)
            }
        };

    public static GenericCategoryScoringRulesOptions CreateCommerceDefaults()
        => new()
        {
            ImportantSubcategories = ["supermarket", "convenience_store", "drogerie"],
            MaxScoreRequiredSubcategories = ["supermarket"],
            DiversitySubcategories = ["supermarket", "convenience_store"],
            DiversityAnyOfGroups = [["drogerie", "mall"], ["electronics", "building_store"]],
            CompletenessRequiredSubcategories = ["supermarket"],
            CompletenessAnyOfGroups = [["convenience_store", "drogerie"]],
            DensityTargetUsefulCount = 10m,
            SingleSubcategoryCap = 55m,
            TwoSubcategoryCap = 80m,
            ThreeSubcategoryCap = 92m,
            MinimumSubcategoriesForMaxScore = 4,
            Subcategories = new(StringComparer.OrdinalIgnoreCase)
            {
                ["supermarket"] = new(1400m, 4, 0.32m, 0.28m),
                ["convenience_store"] = new(1000m, 5, 0.24m, 0.24m),
                ["drogerie"] = new(1600m, 3, 0.16m, 0.16m),
                ["mall"] = new(3000m, 1, 0.12m, 0.10m),
                ["electronics"] = new(2500m, 1, 0.08m, 0.08m),
                ["building_store"] = new(2500m, 1, 0.08m, 0.08m),
                ["beauty_service"] = new(2200m, 4, 0.12m, 0.12m)
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
                ["college"] = new(4000m, 2, 0.14m, 0.16m),
                ["university"] = new(5000m, 2, 0.12m, 0.12m)
            }
        };

    public static GenericCategoryScoringRulesOptions CreateLeisureDefaults()
        => new()
        {
            ImportantSubcategories = ["park", "fitness", "museum", "theater", "cinema", "landmark"],
            MaxScoreRequiredSubcategories = ["park", "fitness"],
            DiversitySubcategories = ["park", "fitness"],
            DiversityAnyOfGroups = [["museum", "theater", "cinema", "landmark"], ["cafe", "restaurant"]],
            CompletenessRequiredSubcategories = ["park", "fitness"],
            CompletenessAnyOfGroups = [["museum", "theater", "cinema", "landmark"]],
            DensityTargetUsefulCount = 7m,
            Subcategories = new(StringComparer.OrdinalIgnoreCase)
            {
                ["park"] = new(1200m, 2, 0.26m, 0.22m),
                ["fitness"] = new(2000m, 2, 0.22m, 0.18m),
                ["museum"] = new(3200m, 1, 0.12m, 0.14m),
                ["theater"] = new(3200m, 1, 0.10m, 0.12m),
                ["cinema"] = new(3200m, 1, 0.10m, 0.12m),
                ["landmark"] = new(3500m, 2, 0.10m, 0.10m),
                ["cafe"] = new(1200m, 3, 0.10m, 0.12m),
                ["restaurant"] = new(1200m, 3, 0.10m, 0.10m),
                ["gallery"] = new(3200m, 1, 0.08m, 0.10m),
                ["exhibition_hall"] = new(3200m, 1, 0.08m, 0.10m),
                ["library"] = new(2800m, 1, 0.08m, 0.10m)
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
    public decimal SurfaceStopsWeight { get; set; } = 58m;
    public decimal MetroWeight { get; set; } = 20m;
    public decimal RailwayWeight { get; set; } = 10m;
    public decimal ParkingWeight { get; set; } = 12m;
    public decimal FuelWeight { get; set; } = 4m;
    public decimal ServiceStationWeight { get; set; } = 4m;
    public decimal DiversityWeightTransport { get; set; } = 10m;
    public decimal CompletenessWeightTransport { get; set; } = 8m;
    public decimal SurfaceNearestWeight { get; set; } = 0.35m;
    public decimal SurfaceCountSaturationWeight { get; set; } = 0.50m;
    public decimal SurfaceModeRichnessWeight { get; set; } = 0.15m;
    public int BusStopGroupUsefulCountCap { get; set; } = 6;
    public int TramStopGroupUsefulCountCap { get; set; } = 3;
    public int TrolleybusStopGroupUsefulCountCap { get; set; } = 3;
    public int MetroUsefulCountCap { get; set; } = 1;
    public int RailwayUsefulCountCap { get; set; } = 1;
    public int ParkingUsefulCountCap { get; set; } = 3;
    public decimal SurfaceStopRadiusMeters { get; set; } = 2000m;
    public decimal MetroRadiusMeters { get; set; } = 4000m;
    public decimal RailwayRadiusMeters { get; set; } = 2000m;
    public decimal ParkingRadiusMeters { get; set; } = 500m;
    public decimal FuelRadiusMeters { get; set; } = 2000m;
    public decimal ServiceStationRadiusMeters { get; set; } = 2000m;
    public decimal CompletenessMaxDistanceMeters { get; set; } = 2000m;
    public decimal SurfaceOnlyCap { get; set; } = 78m;
    public decimal SurfacePlusParkingCap { get; set; } = 88m;
    public decimal SurfacePlusParkingAndRailwayCap { get; set; } = 95m;
    public decimal SurfacePlusParkingAndMetroCap { get; set; } = 100m;
    public decimal NonMetroMaxCapWithSurfaceAndParking { get; set; } = 100m;
    public decimal SurfaceUsefulCountTarget { get; set; } = 6m;
}
