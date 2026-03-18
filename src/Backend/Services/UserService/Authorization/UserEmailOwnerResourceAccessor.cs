using BasicBuildingBlocks.Authorization;
using BasicBuildingBlocks.Services.Abstractions;

namespace UserService.Authorization;

public class UserEmailOwnerResourceAccessor(ICurrentUserProvider currentUserProvider) : IOwnerResourceAccessor<string>
{
    public bool IsOwner(string resource)
        => string.Equals(currentUserProvider.GetEmail(), resource, StringComparison.Ordinal);
}
