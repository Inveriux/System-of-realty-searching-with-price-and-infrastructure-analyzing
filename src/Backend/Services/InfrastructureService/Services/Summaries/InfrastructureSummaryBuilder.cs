using InfrastructureService.Models;
using InfrastructureService.Services.Summaries.Abstractions;

namespace InfrastructureService.Services.Summaries;

public sealed class InfrastructureSummaryBuilder : IInfrastructureSummaryBuilder
{
    public string BuildSummary(InfrastructureScore score, IReadOnlyCollection<LocationPoiSnapshotItem> items)
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

        var strengths = BuildHighlights(score, items);
        return strengths.Count == 0
            ? level
            : $"{level} {string.Join(" ", strengths.Take(2))}";
    }

    public IReadOnlyCollection<string> BuildHighlights(InfrastructureScore score, IReadOnlyCollection<LocationPoiSnapshotItem> items)
    {
        var highlights = new List<string>();

        if (score.TransportScore >= 65m)
            highlights.Add("Хорошая транспортная доступность.");

        if (score.CommerceScore >= 60m)
            highlights.Add("Рядом есть магазины и повседневная коммерция.");

        if (score.HealthcareScore >= 55m)
            highlights.Add("В пешей доступности есть медицинские объекты.");

        if (score.EducationScore >= 55m)
            highlights.Add("В районе достаточно объектов образования.");

        if (score.LeisureScore >= 50m)
            highlights.Add("Есть места для отдыха и досуга.");

        if (highlights.Count == 0 && items.Count > 0)
            highlights.Add($"В пределах радиуса найдено {items.Count} объектов инфраструктуры.");

        return highlights;
    }
}
