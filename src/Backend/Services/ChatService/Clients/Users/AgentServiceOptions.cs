namespace ChatService.Clients.Users;

public sealed class AgentServiceOptions
{
    public const string SectionName = "AgentService";

    public string BaseUrl { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 8;
}
