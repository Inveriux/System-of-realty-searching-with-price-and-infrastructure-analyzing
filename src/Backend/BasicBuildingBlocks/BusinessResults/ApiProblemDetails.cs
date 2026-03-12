using Microsoft.AspNetCore.Mvc;

namespace BasicBuildingBlocks.BusinessResults;

public sealed class ApiProblemDetails : ProblemDetails
{
    public List<ApiErrorItem> Errors { get; init; } = [];
}

public sealed record ApiErrorItem(string Message, int? Code = null, string? PropertyName = null);
