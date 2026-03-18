using ListingService.Application.UseCases.RoadAccessTypes;
using BasicBuildingBlocks.BusinessResults.Extensoins;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;

public class RoadAccessTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/road_access_types")
            .WithTags("Road Access Types");

        group.MapGet("/", GetAllRoadAccessTypes)
            .WithName("GetRoadAccessTypes")
            .WithSummary("Gets all road access types.");
        group.MapPost("/", CreateRoadAccessType)
            .WithName("CreateRoadAccessType")
            .WithSummary("Creates a road access type.");
        group.MapDelete("/{name}", DeleteRoadAccessType)
            .WithName("DeleteRoadAccessType")
            .WithSummary("Deletes a road access type by name.");

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
