using ListingService.Application.UseCases.WallMaterials;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BasicBuildingBlocks.BusinessResults.Extensoins;

public class WallMaterialsEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/wall_materials")
            .WithTags("Wall Materials");

        group.MapGet("/", GetAllWallMaterials)
            .WithName("GetWallMaterials")
            .WithSummary("Gets all wall materials.");
        group.MapPost("/", CreateWallMaterial)
            .WithName("CreateWallMaterial")
            .WithSummary("Creates a wall material.");
        group.MapDelete("/{name}", DeleteWallMaterial)
            .WithName("DeleteWallMaterial")
            .WithSummary("Deletes a wall material by name.");

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
