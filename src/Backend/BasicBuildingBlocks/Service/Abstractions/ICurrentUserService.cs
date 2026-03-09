namespace BasicBuildingBlocks.Service.Abstractions;

public interface ICurrentUserProvider
{
    string GetRole();
    Guid GetUserId();
    string GetUserName();
    string GetEmail(); 
    string GetGoogleId();
    string? GetImageUrl();
}