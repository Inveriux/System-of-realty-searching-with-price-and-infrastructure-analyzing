namespace InfrastructureService.Services.Yandex.Abstractions;

public interface IYandexOrganizationSearchClient
{
    Task<IReadOnlyCollection<YandexOrganizationSearchResult>> SearchAsync(
        decimal latitude,
        decimal longitude,
        string searchText,
        int radiusMeters,
        CancellationToken cancellationToken);
}
