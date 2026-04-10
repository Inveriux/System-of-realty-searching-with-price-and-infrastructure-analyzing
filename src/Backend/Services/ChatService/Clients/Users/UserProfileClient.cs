using System.Net;
using System.Text.Json;
using ChatService.Clients.Users.Abstractions;

namespace ChatService.Clients.Users;

public sealed class UserProfileClient(HttpClient httpClient) : IUserProfileClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<UserProfileSnapshot?> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"api/agents/{userId}/public-profile", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var envelope = await JsonSerializer.DeserializeAsync<ApiEnvelope<DownstreamSellerPublicProfileDto>>(stream, JsonOptions, cancellationToken);
        if (envelope?.IsSuccess != true || envelope.Data is null)
        {
            return null;
        }

        return new UserProfileSnapshot(
            envelope.Data.Id,
            envelope.Data.FullName,
            envelope.Data.AvatarUrl,
            envelope.Data.Email,
            envelope.Data.Phone);
    }

    public async Task<UserProfileSnapshot?> GetSupportProfileAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync("api/agents/support-profile", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var envelope = await JsonSerializer.DeserializeAsync<ApiEnvelope<DownstreamSellerPublicProfileDto>>(stream, JsonOptions, cancellationToken);
        if (envelope?.IsSuccess != true || envelope.Data is null)
        {
            return null;
        }

        return new UserProfileSnapshot(
            envelope.Data.Id,
            envelope.Data.FullName,
            envelope.Data.AvatarUrl,
            envelope.Data.Email,
            envelope.Data.Phone);
    }
}

public sealed record UserProfileSnapshot(Guid Id, string FullName, string? AvatarUrl, string? Email, string? Phone);

internal sealed class ApiEnvelope<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
}

internal sealed class DownstreamSellerPublicProfileDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
}
