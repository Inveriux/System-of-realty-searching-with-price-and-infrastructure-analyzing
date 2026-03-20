using AutoMapper;
using InfrastructureService.Models;
using System.Text.Json;

namespace InfrastructureService.DTOs.Infrastructure;

public sealed class InfrastructureScoreDto
{
    public Guid LocationId { get; init; }
    public Guid SnapshotId { get; init; }
    public decimal TotalScore { get; init; }
    public decimal EducationScore { get; init; }
    public decimal HealthcareScore { get; init; }
    public decimal TransportScore { get; init; }
    public decimal CommerceScore { get; init; }
    public decimal LeisureScore { get; init; }
    public int PoiCount { get; init; }
    public DateTime CalculatedAtUtc { get; init; }
    public Dictionary<string, decimal> SearchFeatures { get; init; } = [];

    private sealed class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<InfrastructureScore, InfrastructureScoreDto>()
                .ForMember(x => x.SearchFeatures, y => y.MapFrom(s =>
                    string.IsNullOrWhiteSpace(s.SearchFeaturesJson)
                        ? new Dictionary<string, decimal>()
                        : JsonSerializer.Deserialize<Dictionary<string, decimal>>(s.SearchFeaturesJson) ?? new Dictionary<string, decimal>()));
        }
    }
}
