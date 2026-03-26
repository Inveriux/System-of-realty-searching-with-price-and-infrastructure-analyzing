using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Models;
using InfrastructureService.Services.Scoring.ScoringEntities;
using InfrastructureService.Services.Scoring.Configuration;

namespace InfrastructureService.Services.Scoring.Abstractions;

public interface IInfrastructureCategoryScoringPolicy
{
    string CategoryCode { get; }
    CategoryScoreBreakdownDto Score(
        Location location,
        IReadOnlyCollection<InfrastructureScoringEntity> entities,
        InfrastructureDetailsDto details,
        InfrastructureScoringProfileOptions profile);
}
