using ListingService.Application.UseCases.WallMaterials;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;

public class WallMaterialsEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder RegisterEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/wall_materials");

        group.MapGet("/", GetAllWallMaterials);
        group.MapPost("/", CreateWallMaterial);
        group.MapDelete("/{name}", DeleteWallMaterial);

        return group;
    }

    public async Task<IResult> GetAllWallMaterials(ISender sender)
    {
        var result = await sender.Send(new GetWallMaterialsQuery());
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateWallMaterial(ISender sender,
        LookupNameDto dto)
    {
        var result = await sender.Send(new CreateWallMaterialCommand(dto.Name));
        return result.ToHttpResult();
    }

    public async Task<IResult> DeleteWallMaterial(ISender sender,
        [FromRoute] string name)
    {
        var result = await sender.Send(new DeleteWallMaterialCommand(name));
        return result.ToHttpResult();
    }
}
