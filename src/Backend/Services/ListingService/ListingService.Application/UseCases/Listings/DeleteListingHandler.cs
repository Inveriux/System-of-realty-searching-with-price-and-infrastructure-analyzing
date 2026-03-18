using MediatR;

namespace ListingService.Application.UseCases.Listings;

public record DeleteListingCommand(Guid Id) : IRequest<Result>;

public class DeleteListingHandler(IRepositoryManager repositoryManager)
    : IRequestHandler<DeleteListingCommand, Result>
{
    public async Task<Result> Handle(DeleteListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await repositoryManager.Listing.GetByIdAsync(request.Id, true, cancellationToken);

        if (listing is null)
        {
            return Result.NotFound(
                "Listing not found.",
                [new ResultError("Listing not found.", 0, nameof(request.Id))]);
        }

        await repositoryManager.Listing.DeleteAsync(request.Id, cancellationToken);
        await repositoryManager.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
