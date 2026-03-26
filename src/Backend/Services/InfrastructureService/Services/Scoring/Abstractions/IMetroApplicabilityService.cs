using InfrastructureService.Models;

namespace InfrastructureService.Services.Scoring.Abstractions;

public interface IMetroApplicabilityService
{
    bool IsApplicable(Location location);
}
