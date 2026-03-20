namespace ListingService.Presentation.Services;

public sealed class ResolvedLocationDto
{
    public Guid Id { get; init; }
    public string AddressText { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
}

public sealed class InfrastructureClientResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
}
