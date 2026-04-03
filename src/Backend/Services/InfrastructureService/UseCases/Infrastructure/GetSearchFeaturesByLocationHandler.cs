using BasicBuildingBlocks.BusinessResults;
using InfrastructureService.Data.Repositories.Abstractions;
using MediatR;
using System.Text.Json;

namespace InfrastructureService.UseCases.Infrastructure;

public sealed record GetSearchFeaturesByLocationQuery(Guid LocationId) : IRequest<Result<Dictionary<string, decimal>>>;

public sealed class GetSearchFeaturesByLocationHandler(IInfrastructureRepository infrastructureRepository)
    : IRequestHandler<GetSearchFeaturesByLocationQuery, Result<Dictionary<string, decimal>>>
{
    public async Task<Result<Dictionary<string, decimal>>> Handle(GetSearchFeaturesByLocationQuery request, CancellationToken cancellationToken)
    {
        var score = await infrastructureRepository.GetScoreAsync(request.LocationId, cancellationToken);
        if (score is null)
            return Result.NotFound<Dictionary<string, decimal>>("Оценка инфраструктуры не найдена.");

        var features = string.IsNullOrWhiteSpace(score.SearchFeaturesJson)
            ? new Dictionary<string, decimal>()
            : JsonSerializer.Deserialize<Dictionary<string, decimal>>(score.SearchFeaturesJson) ?? new Dictionary<string, decimal>();

        return Result.Ok(features);
    }
}
