using InfrastructureService.Services.Places.Abstractions;

namespace InfrastructureService.Services.Classification.Abstractions;

public interface IPoiClassificationService
{
    PoiClassificationResult? Classify(ExternalPoiResult poi);
    Dictionary<string, string> GetDictionaryVersions();
}
