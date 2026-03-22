using BasicBuildingBlocks.BusinessResults;
using SearchService.Data.Repositories.Abstractions;
using SearchService.Services.InternalClients.Abstractions;
using MediatR;

namespace SearchService.UseCases.Indexing;

public sealed record ReindexLocationCommand(Guid LocationId) : IRequest<Result>;

public sealed class ReindexLocationHandler(
    ISearchListingRepository repository,
    IInfrastructureServiceClient infrastructureServiceClient) : IRequestHandler<ReindexLocationCommand, Result>
{
    public async Task<Result> Handle(ReindexLocationCommand request, CancellationToken cancellationToken)
    {
        var listings = await repository.GetByLocationIdAsync(request.LocationId, cancellationToken);
        var infrastructureScore = await infrastructureServiceClient.GetInfrastructureScoreAsync(request.LocationId, cancellationToken);

        foreach (var listing in listings)
        {
            SearchListingInfrastructureMapper.Apply(listing, infrastructureScore);
            listing.UpdatedAtUtc = DateTime.UtcNow;
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
