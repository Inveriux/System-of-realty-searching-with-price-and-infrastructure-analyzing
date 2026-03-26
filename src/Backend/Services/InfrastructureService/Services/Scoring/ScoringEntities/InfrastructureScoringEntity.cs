namespace InfrastructureService.Services.Scoring.ScoringEntities;

public abstract record InfrastructureScoringEntity(
    string CategoryCode,
    string SubcategoryCode,
    decimal Latitude,
    decimal Longitude,
    decimal DistanceMeters,
    string? Name,
    int SourceCount);

public sealed record TransportScoringEntity(
    string SubcategoryCode,
    decimal Latitude,
    decimal Longitude,
    decimal DistanceMeters,
    string? Name,
    int SourceCount,
    string GroupCode)
    : InfrastructureScoringEntity("transport", SubcategoryCode, Latitude, Longitude, DistanceMeters, Name, SourceCount);

public sealed record HealthcareScoringEntity(
    string SubcategoryCode,
    decimal Latitude,
    decimal Longitude,
    decimal DistanceMeters,
    string? Name,
    int SourceCount)
    : InfrastructureScoringEntity("healthcare", SubcategoryCode, Latitude, Longitude, DistanceMeters, Name, SourceCount);

public sealed record CommerceScoringEntity(
    string SubcategoryCode,
    decimal Latitude,
    decimal Longitude,
    decimal DistanceMeters,
    string? Name,
    int SourceCount)
    : InfrastructureScoringEntity("commerce", SubcategoryCode, Latitude, Longitude, DistanceMeters, Name, SourceCount);

public sealed record EducationScoringEntity(
    string SubcategoryCode,
    decimal Latitude,
    decimal Longitude,
    decimal DistanceMeters,
    string? Name,
    int SourceCount)
    : InfrastructureScoringEntity("education", SubcategoryCode, Latitude, Longitude, DistanceMeters, Name, SourceCount);

public sealed record LeisureScoringEntity(
    string SubcategoryCode,
    decimal Latitude,
    decimal Longitude,
    decimal DistanceMeters,
    string? Name,
    int SourceCount)
    : InfrastructureScoringEntity("leisure", SubcategoryCode, Latitude, Longitude, DistanceMeters, Name, SourceCount);
