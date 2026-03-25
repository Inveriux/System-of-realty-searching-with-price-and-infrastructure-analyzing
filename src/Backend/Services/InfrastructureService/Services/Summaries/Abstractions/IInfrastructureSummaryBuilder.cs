using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Models;

namespace InfrastructureService.Services.Summaries.Abstractions;

public interface IInfrastructureSummaryBuilder
{
    string BuildSummary(InfrastructureScore score, InfrastructureDetailsDto details, IReadOnlyCollection<LocationPoiSnapshotItem> items);
    IReadOnlyCollection<string> BuildHighlights(InfrastructureScore score, InfrastructureDetailsDto details, IReadOnlyCollection<LocationPoiSnapshotItem> items);
}
