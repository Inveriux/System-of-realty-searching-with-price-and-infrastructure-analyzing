namespace Backend.IntegrationTests.Common;

internal sealed class ResultEnvelope<T>
{
    public bool IsSuccess { get; init; }
    public string? Message { get; init; }
    public int StatusCode { get; init; }
    public T? Data { get; init; }
}
