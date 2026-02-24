using ListingService.Application.UseCases.RoadAccessTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;

public class RoadAccessTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder RegisterEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/road_access_types");

        group.MapGet("/", GetAllRoadAccessTypes);
        group.MapPost("/", CreateRoadAccessType);
        group.MapDelete("/{name}", DeleteRoadAccessType);

        return group;
    }

    public async Task<IResult> GetAllRoadAccessTypes(ISender sender)
    {
        var result = await sender.Send(new GetRoadAccessTypesQuery());
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateRoadAccessType(ISender sender,
        LookupNameDto dto)
    {
        var result = await sender.Send(new CreateRoadAccessTypeCommand(dto.Name));
        return result.ToHttpResult();
    }

    public async Task<IResult> DeleteRoadAccessType(ISender sender,
        [FromRoute] string name)
    {
        var result = await sender.Send(new DeleteRoadAccessTypeCommand(name));
        return result.ToHttpResult();
    }
}
