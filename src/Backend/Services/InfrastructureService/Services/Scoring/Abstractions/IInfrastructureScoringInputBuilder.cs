using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Models;
using InfrastructureService.Services.Scoring.ScoringEntities;

namespace InfrastructureService.Services.Scoring.Abstractions;

public interface IInfrastructureScoringInputBuilder
{
    InfrastructureScoringInput Build(LocationPoiSnapshot snapshot, InfrastructureDetailsDto details);
}
