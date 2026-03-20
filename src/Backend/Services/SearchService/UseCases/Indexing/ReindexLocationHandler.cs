using BasicBuildingBlocks.BusinessResults;
using SearchService.Data.Repositories.Abstractions;
using SearchService.Services.InternalClients.Abstractions;
using MediatR;
using System.Text.Json;

namespace SearchService.UseCases.Indexing;

public sealed record ReindexLocationCommand(Guid LocationId) : IRequest<Result>;

public sealed class ReindexLocationHandler(
    ISearchListingRepository repository,
    IInfrastructureServiceClient infrastructureServiceClient) : IRequestHandler<ReindexLocationCommand, Result>
{
    public async Task<Result> Handle(ReindexLocationCommand request, CancellationToken cancellationToken)
    {
        var listings = await repository.GetByLocationIdAsync(request.LocationId, cancellationToken);
        var features = await infrastructureServiceClient.GetSearchFeaturesAsync(request.LocationId, cancellationToken);
        var score = features.Count == 0 ? (decimal?)null : features.Values.Sum();

        foreach (var listing in listings)
        {
            listing.InfrastructureScore = score;
            listing.SearchFeaturesJson = JsonSerializer.Serialize(features);
            listing.UpdatedAtUtc = DateTime.UtcNow;
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
