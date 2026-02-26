using ListingService.Application.UseCases.BuildingTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;

public class BuildingTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/building_types");

        group.MapGet("/", GetAllBuildingTypes);
        group.MapPost("/", CreateBuildingType);
        group.MapDelete("/{name}", DeleteBuildingType);

        return group;
    }

    public async Task<IResult> GetAllBuildingTypes(ISender sender)
    {
        var result = await sender.Send(new GetBuildingTypesQuery());
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateBuildingType(ISender sender,
        LookupNameDto dto)
    {
        var result = await sender.Send(new CreateBuildingTypeCommand(dto.Name));
        return result.ToHttpResult();
    }

    public async Task<IResult> DeleteBuildingType(ISender sender,
        [FromRoute] string name)
    {
        var result = await sender.Send(new DeleteBuildingTypeCommand(name));
        return result.ToHttpResult();
    }
}
