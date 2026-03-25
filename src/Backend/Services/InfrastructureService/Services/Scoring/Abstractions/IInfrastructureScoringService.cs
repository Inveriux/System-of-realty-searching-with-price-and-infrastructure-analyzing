using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Models;

namespace InfrastructureService.Services.Scoring.Abstractions;

public interface IInfrastructureScoringService
{
    InfrastructureScore Calculate(Location location, LocationPoiSnapshot snapshot, InfrastructureDetailsDto details);
}
