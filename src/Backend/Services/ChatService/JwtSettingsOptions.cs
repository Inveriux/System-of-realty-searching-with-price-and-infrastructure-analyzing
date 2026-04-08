namespace ChatService;

public sealed class JwtSettingsOptions
{
    public const string SectionName = "Authentication:DomainJwtAuth";

    public string Authority { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
}
