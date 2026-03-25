namespace InfrastructureService.Services.Configuration;

public static class InfrastructureCategoryCodes
{
    public const string Commerce = "commerce";
    public const string Healthcare = "healthcare";
    public const string Transport = "transport";
    public const string Education = "education";
    public const string Leisure = "leisure";

    public static readonly IReadOnlyCollection<string> All =
    [
        Commerce,
        Healthcare,
        Transport,
        Education,
        Leisure
    ];
}
