namespace BasicBuildingBlocks.Authorization;

public interface IOwnerResourceAccessor<in TResource>
{
    bool IsOwner(TResource resource);
}
