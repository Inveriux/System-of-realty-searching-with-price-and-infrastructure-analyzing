using BasicBuildingBlocks.BusinessResults.Extensoins;
using BasicBuildingBlocks.Endpoints.Abstractions;
using InfrastructureService.DTOs.Locations;
using InfrastructureService.UseCases.Locations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureService.Endpoints;

public sealed class LocationEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var publicGroup = application.MapGroup("/api/locations")
            .WithTags("Locations");

        publicGroup.MapPost("/resolve", ResolveAsync);
        publicGroup.MapGet("/{id:guid}", GetByIdAsync);
        publicGroup.MapGet("/by-coordinates", GetByCoordinatesAsync);

        var internalGroup = application.MapGroup("/internal/locations")
            .WithTags("Internal Locations");

        internalGroup.MapPost("/resolve", ResolveAsync);
        return publicGroup;
    }

    public async Task<IResult> ResolveAsync(ISender sender, ResolveLocationRequestDto dto, CancellationToken cancellationToken)
        => (await sender.Send(new ResolveLocationCommand(dto), cancellationToken)).ToHttpResult();

    public async Task<IResult> GetByIdAsync(ISender sender, [FromRoute] Guid id, CancellationToken cancellationToken)
        => (await sender.Send(new GetLocationByIdQuery(id), cancellationToken)).ToHttpResult();

    public async Task<IResult> GetByCoordinatesAsync(
        ISender sender,
        [FromQuery] decimal latitude,
        [FromQuery] decimal longitude,
        CancellationToken cancellationToken)
        => (await sender.Send(new GetLocationByCoordinatesQuery(latitude, longitude), cancellationToken)).ToHttpResult();
}
