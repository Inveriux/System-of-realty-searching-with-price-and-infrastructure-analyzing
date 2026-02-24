using ListingService.Application.UseCases.BuildingMaterials;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;

public class BuildingMaterialsEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder RegisterEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/building_materials");

        group.MapGet("/", GetAllBuildingMaterials);
        group.MapPost("/", CreateBuildingMaterial);
        group.MapDelete("/{name}", DeleteBuildingMaterial);

        return group;
    }

    public async Task<IResult> GetAllBuildingMaterials(ISender sender)
    {
        var result = await sender.Send(new GetBuildingMaterialsQuery());
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateBuildingMaterial(ISender sender,
        LookupNameDto dto)
    {
        var result = await sender.Send(new CreateBuildingMaterialCommand(dto.Name));
        return result.ToHttpResult();
    }

    public async Task<IResult> DeleteBuildingMaterial(ISender sender,
        [FromRoute] string name)
    {
        var result = await sender.Send(new DeleteBuildingMaterialCommand(name));
        return result.ToHttpResult();
    }
}
