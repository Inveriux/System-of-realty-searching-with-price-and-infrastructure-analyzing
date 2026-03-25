using System.Text.Json;
using InfrastructureService.Services.Configuration;

namespace InfrastructureService.Services.Classification;

public sealed class PoiClassificationDictionaryProvider
{
    private readonly Lazy<ClassificationResources> _resources;

    public PoiClassificationDictionaryProvider()
    {
        _resources = new Lazy<ClassificationResources>(LoadResources, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public ClassificationResources GetResources() => _resources.Value;

    private static ClassificationResources LoadResources()
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "Resources", "Classification");
        return new ClassificationResources(
            Load<BrandDictionaryRoot>(basePath, "poi-brand-dictionary.json"),
            Load<KeywordDictionaryRoot>(basePath, "poi-keyword-dictionary.json"),
            Load<StopKeywordDictionaryRoot>(basePath, "poi-stop-keyword-dictionary.json"),
            Load<ExcludeKeywordDictionaryRoot>(basePath, "poi-exclude-keyword-dictionary.json"));
    }

    private static T Load<T>(string basePath, string fileName)
    {
        var fullPath = Path.Combine(basePath, fileName);
        using var stream = File.OpenRead(fullPath);
        return JsonSerializer.Deserialize<T>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException($"Failed to deserialize classification resource '{fileName}'.");
    }
}

public sealed record ClassificationResources(
    BrandDictionaryRoot BrandDictionary,
    KeywordDictionaryRoot KeywordDictionary,
    StopKeywordDictionaryRoot StopKeywordDictionary,
    ExcludeKeywordDictionaryRoot ExcludeKeywordDictionary)
{
    public Dictionary<string, string> GetVersions()
        => new(StringComparer.OrdinalIgnoreCase)
        {
            ["poi-brand-dictionary"] = BrandDictionary.Version,
            ["poi-keyword-dictionary"] = KeywordDictionary.Version,
            ["poi-stop-keyword-dictionary"] = StopKeywordDictionary.Version,
            ["poi-exclude-keyword-dictionary"] = ExcludeKeywordDictionary.Version
        };
}

public sealed class BrandDictionaryRoot
{
    public string Version { get; init; } = "unknown";
    public Dictionary<string, string[]> Commerce { get; init; } = [];
    public Dictionary<string, string[]> Healthcare { get; init; } = [];
    public Dictionary<string, string[]> Transport { get; init; } = [];
    public Dictionary<string, string[]> Education { get; init; } = [];
    public Dictionary<string, string[]> Leisure { get; init; } = [];

    public IEnumerable<(string Category, string Subcategory, string Alias)> EnumerateAliases()
    {
        foreach (var (category, aliases) in EnumerateCategoryMaps())
        {
            foreach (var (subcategory, items) in aliases)
            {
                foreach (var alias in items)
                    yield return (category, subcategory, alias);
            }
        }
    }

    private IEnumerable<(string Category, Dictionary<string, string[]> Aliases)> EnumerateCategoryMaps()
    {
        yield return (InfrastructureCategoryCodes.Commerce, Commerce);
        yield return (InfrastructureCategoryCodes.Healthcare, Healthcare);
        yield return (InfrastructureCategoryCodes.Transport, Transport);
        yield return (InfrastructureCategoryCodes.Education, Education);
        yield return (InfrastructureCategoryCodes.Leisure, Leisure);
    }
}

public sealed class KeywordDictionaryRoot
{
    public string Version { get; init; } = "unknown";
    public IReadOnlyCollection<KeywordDictionaryEntry> Keywords { get; init; } = [];
}

public sealed class KeywordDictionaryEntry
{
    public string Keyword { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Subcategory { get; init; } = string.Empty;
}

public sealed class StopKeywordDictionaryRoot
{
    public string Version { get; init; } = "unknown";
    public IReadOnlyCollection<string> StopKeywords { get; init; } = [];
}

public sealed class ExcludeKeywordDictionaryRoot
{
    public string Version { get; init; } = "unknown";
    public IReadOnlyCollection<string> ExcludeKeywords { get; init; } = [];
}
