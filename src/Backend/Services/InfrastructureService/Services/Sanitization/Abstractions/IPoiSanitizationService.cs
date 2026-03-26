using InfrastructureService.Services.Places.Abstractions;

namespace InfrastructureService.Services.Sanitization.Abstractions;

public interface IPoiSanitizationService
{
    PoiSanitizationResult Evaluate(ExternalPoiResult poi);
}
