namespace SearchService.DTOs.Search;

public sealed class SearchRankingWeightsDto
{
    public decimal TransportWeight { get; init; }
    public decimal EducationWeight { get; init; }
    public decimal HealthcareWeight { get; init; }
    public decimal CommerceWeight { get; init; }
    public decimal LeisureWeight { get; init; }
    public decimal PriceWeight { get; init; }
    public decimal LocationWeight { get; init; }
    public decimal AreaWeight { get; init; }
}
