namespace InfrastructureService.Services.Yandex.Abstractions;

public interface IYandexGeocoderClient
{
    Task<YandexResolvedLocation?> ResolveAsync(string addressText, CancellationToken cancellationToken);
}
