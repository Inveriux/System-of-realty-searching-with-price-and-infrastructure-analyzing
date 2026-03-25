using InfrastructureService.Models;

namespace InfrastructureService.Services.TransportGrouping.Abstractions;

public interface ITransportStopGroupingService
{
    string NormalizeStopName(string? name);
    IReadOnlyCollection<TransportStopGroup> Group(IReadOnlyCollection<LocationPoiSnapshotItem> items);
}
