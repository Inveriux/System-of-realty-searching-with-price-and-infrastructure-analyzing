using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Models;
using InfrastructureService.Services.Summaries.Abstractions;

namespace InfrastructureService.Services.Summaries;

public sealed class InfrastructureSummaryBuilder : IInfrastructureSummaryBuilder
{
    public string BuildSummary(InfrastructureScore score, InfrastructureDetailsDto details, IReadOnlyCollection<LocationPoiSnapshotItem> items)
    {
        if (items.Count == 0)
            return "Инфраструктурные объекты рядом не найдены или внешний провайдер не вернул данные.";

        var level = score.TotalScore switch
        {
            >= 80m => "Инфраструктура развита значительно выше среднего.",
            >= 65m => "Инфраструктура развита выше среднего.",
            >= 45m => "Инфраструктура развита на среднем уровне.",
            > 0m => "Инфраструктура рядом ограниченная, но базовые объекты присутствуют.",
            _ => "Инфраструктурная оценка пока не рассчитана."
        };

        var highlights = BuildHighlights(score, details, items);
        if (details.MissingPrimaryCategories.Count == 0)
            return highlights.Count == 0 ? level : $"{level} {string.Join(" ", highlights.Take(2))}";

        var missing = string.Join(", ", details.MissingPrimaryCategories.Take(3).Select(TranslatePrimaryCategory));
        return $"{level} В основной зоне не найдены: {missing}.";
    }

    public IReadOnlyCollection<string> BuildHighlights(InfrastructureScore score, InfrastructureDetailsDto details, IReadOnlyCollection<LocationPoiSnapshotItem> items)
    {
        var highlights = new List<string>();
        var nearbyCategories = details.NearbyCategories.ToDictionary(
            static category => category.SubcategoryCode,
            StringComparer.OrdinalIgnoreCase);

        if (score.TransportScore >= 65m)
            highlights.Add("Хорошая транспортная доступность.");

        if (details.SearchFeatures.NearestStopGroupWalkMinutes is not null)
        {
            var stopLabel = string.IsNullOrWhiteSpace(details.SearchFeatures.NearestStopGroupName)
                ? "Ближайшая остановка"
                : $"Ближайшая остановка \"{details.SearchFeatures.NearestStopGroupName}\"";
            highlights.Add($"{stopLabel}: {details.SearchFeatures.NearestStopGroupWalkMinutes:0.#} мин пешком.");
        }

        if (details.SearchFeatures.StopGroupsCountPrimary > 0)
            highlights.Add($"Рядом остановочных групп: {details.SearchFeatures.StopGroupsCountPrimary}.");

        if (details.SearchFeatures.NearestStopGroupPlatformCount is > 1)
            highlights.Add($"В ближайшей остановочной группе платформ: {details.SearchFeatures.NearestStopGroupPlatformCount}.");

        if (details.SearchFeatures.ParkingCountPrimary > 0 && details.SearchFeatures.NearestParkingWalkMinutes is not null)
            highlights.Add($"Парковка рядом: {details.SearchFeatures.NearestParkingWalkMinutes:0.#} мин пешком.");

        if (details.SearchFeatures.NearestStoreWalkMinutes is not null)
            highlights.Add($"Ближайший супермаркет: {details.SearchFeatures.NearestStoreWalkMinutes:0.#} мин пешком.");

        if (details.SearchFeatures.NearestPharmacyWalkMinutes is not null)
            highlights.Add($"Ближайшая аптека: {details.SearchFeatures.NearestPharmacyWalkMinutes:0.#} мин пешком.");

        if (nearbyCategories.TryGetValue("school", out var schoolCategory) && schoolCategory.ReachableWithin15MinCount > 0)
            highlights.Add($"Школ в 15 мин пешком: {schoolCategory.ReachableWithin15MinCount}.");

        if (nearbyCategories.TryGetValue("kindergarten", out var kindergartenCategory) && kindergartenCategory.ReachableWithin15MinCount > 0)
            highlights.Add($"Детских садов в 15 мин пешком: {kindergartenCategory.ReachableWithin15MinCount}.");

        if (score.CommerceScore >= 60m)
            highlights.Add("Рядом есть магазины и повседневная коммерция.");

        if (score.HealthcareScore >= 55m)
            highlights.Add("В пешей доступности есть медицинские объекты.");

        if (score.EducationScore >= 55m)
            highlights.Add("В районе достаточно объектов образования.");

        if (details.SearchFeatures.NearestParkWalkMinutes is not null)
            highlights.Add($"Парк: {details.SearchFeatures.NearestParkWalkMinutes:0.#} мин пешком.");
        else if (details.SearchFeatures.NearestParkAnyDistanceM is not null)
            highlights.Add($"Парк вне основной зоны: {details.SearchFeatures.NearestParkAnyDistanceM / 1000m:0.#} км.");

        if (details.SearchFeatures.NearestFitnessWalkMinutes is not null)
            highlights.Add($"Фитнес рядом: {details.SearchFeatures.NearestFitnessWalkMinutes:0.#} мин пешком.");
        else if (details.SearchFeatures.NearestFitnessDriveMinutes is not null)
            highlights.Add($"Фитнес рядом: {details.SearchFeatures.NearestFitnessDriveMinutes:0.#} мин на машине.");

        if (details.SearchFeatures.NearestMuseumDriveMinutes is not null)
            highlights.Add($"Музей: {details.SearchFeatures.NearestMuseumDriveMinutes:0.#} мин на машине.");
        else if (details.SearchFeatures.NearestTheaterDriveMinutes is not null)
            highlights.Add($"Театр: {details.SearchFeatures.NearestTheaterDriveMinutes:0.#} мин на машине.");
        else if (details.SearchFeatures.NearestCinemaDriveMinutes is not null)
            highlights.Add($"Кинотеатр: {details.SearchFeatures.NearestCinemaDriveMinutes:0.#} мин на машине.");
        else if (score.LeisureScore >= 50m)
            highlights.Add("Есть места для отдыха, спорта и культурного досуга.");

        if (details.SearchFeatures.MetroFoundInPrimaryRadius && details.SearchFeatures.NearestMetroWalkMinutes is not null)
            highlights.Add($"Метро рядом: {details.SearchFeatures.NearestMetroWalkMinutes:0.#} мин пешком.");

        if (details.MissingPrimaryCategories.Count > 0)
            highlights.Add($"В основной зоне недостаёт: {string.Join(", ", details.MissingPrimaryCategories.Take(3).Select(TranslatePrimaryCategory))}.");

        if (highlights.Count == 0 && items.Count > 0)
            highlights.Add($"В пределах радиуса найдено {items.Count} объектов инфраструктуры.");

        return highlights;
    }

    private static string TranslatePrimaryCategory(string value) => value switch
    {
        "store" => "магазины",
        "pharmacy" => "аптеки",
        "stop" => "остановки",
        "parking" => "парковки",
        "school" => "школы",
        "kindergarten" => "детские сады",
        "clinic" => "поликлиники",
        "hospital" => "больницы",
        "leisure" => "досуг",
        "park" => "парки",
        "fitness" => "фитнес",
        "museum" => "музеи",
        "landmark" => "достопримечательности",
        "theater" => "театры",
        "cinema" => "кинотеатры",
        "metro_station" => "метро",
        _ => value
    };
}
