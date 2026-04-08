namespace ChatService.Clients.Users.Abstractions;

public interface IUserProfileClient
{
    Task<UserProfileSnapshot?> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken);
}
