using BasicBuildingBlocks.BusinessResults;
using SearchService.Data.Repositories.Abstractions;
using MediatR;

namespace SearchService.UseCases.Indexing;

public sealed record DeleteIndexedListingCommand(Guid ListingId) : IRequest<Result>;

public sealed class DeleteIndexedListingHandler(ISearchListingRepository repository) : IRequestHandler<DeleteIndexedListingCommand, Result>
{
    public async Task<Result> Handle(DeleteIndexedListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await repository.GetByListingIdAsync(request.ListingId, cancellationToken);
        if (listing is null)
            return Result.NotFound("Indexed listing was not found.");

        repository.Remove(listing);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
