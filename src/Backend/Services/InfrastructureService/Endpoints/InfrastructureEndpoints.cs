using BasicBuildingBlocks.BusinessResults.Extensoins;
using BasicBuildingBlocks.Endpoints.Abstractions;
using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.UseCases.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureService.Endpoints;

public sealed class InfrastructureEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var publicGroup = application.MapGroup("/api/infrastructure")
            .WithTags("Infrastructure");

        publicGroup.MapPost("/refresh", RefreshAsync);
        publicGroup.MapGet("/{locationId:guid}", GetScoreAsync);
        publicGroup.MapGet("/{locationId:guid}/pois", GetPoisAsync);
        publicGroup.MapGet("/{locationId:guid}/summary", GetSummaryAsync);

        var internalGroup = application.MapGroup("/internal/infrastructure")
            .WithTags("Internal Infrastructure");

        internalGroup.MapPost("/refresh-for-location", RefreshAsync);
        internalGroup.MapGet("/{locationId:guid}/search-features", GetSearchFeaturesAsync);

        return publicGroup;
    }

    public async Task<IResult> RefreshAsync(ISender sender, RefreshInfrastructureRequestDto dto, CancellationToken cancellationToken)
        => (await sender.Send(new RefreshInfrastructureCommand(dto), cancellationToken)).ToHttpResult();

    public async Task<IResult> GetScoreAsync(ISender sender, [FromRoute] Guid locationId, CancellationToken cancellationToken)
        => (await sender.Send(new GetInfrastructureByLocationQuery(locationId), cancellationToken)).ToHttpResult();

    public async Task<IResult> GetPoisAsync(ISender sender, [FromRoute] Guid locationId, CancellationToken cancellationToken)
        => (await sender.Send(new GetInfrastructurePoisByLocationQuery(locationId), cancellationToken)).ToHttpResult();

    public async Task<IResult> GetSummaryAsync(ISender sender, [FromRoute] Guid locationId, CancellationToken cancellationToken)
        => (await sender.Send(new GetInfrastructureSummaryByLocationQuery(locationId), cancellationToken)).ToHttpResult();

    public async Task<IResult> GetSearchFeaturesAsync(ISender sender, [FromRoute] Guid locationId, CancellationToken cancellationToken)
        => (await sender.Send(new GetSearchFeaturesByLocationQuery(locationId), cancellationToken)).ToHttpResult();
}
