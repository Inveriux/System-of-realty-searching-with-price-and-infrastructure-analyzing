using InfrastructureService.Models;
using InfrastructureService.Services.Places;
using InfrastructureService.Services.Scoring.Abstractions;
using Microsoft.Extensions.Options;

namespace InfrastructureService.Services.Scoring;

public sealed class MetroApplicabilityService(IOptions<InfrastructureScoringOptions> options) : IMetroApplicabilityService
{
    public bool IsApplicable(Location location)
    {
        var cityCandidates = new[]
        {
            location.City,
            location.AddressText,
            location.NormalizedAddressText
        };

        return cityCandidates
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Any(candidate => options.Value.MetroApplicableCities.Any(city =>
                candidate!.Contains(city, StringComparison.OrdinalIgnoreCase)));
    }
}
