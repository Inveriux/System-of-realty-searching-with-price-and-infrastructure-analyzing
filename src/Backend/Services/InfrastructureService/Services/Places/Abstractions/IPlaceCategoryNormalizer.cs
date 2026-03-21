namespace InfrastructureService.Services.Places.Abstractions;

public interface IPlaceCategoryNormalizer
{
    string? Normalize(IReadOnlyDictionary<string, string> tags);
}
