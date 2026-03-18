using BasicBuildingBlocks.Authorization;
using BasicBuildingBlocks.Services.Abstractions;

namespace ListingService.Presentation.Authorization;

public class ListingOwnerResourceAccessor(ICurrentUserProvider currentUserProvider) : IOwnerResourceAccessor<Guid>
{
    public bool IsOwner(Guid resource)
        => currentUserProvider.GetUserId() == resource;
}
