using ListingService.Application.UseCases.BuildingMaterials;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BasicBuildingBlocks.BusinessResults.Extensoins;

public class BuildingMaterialsEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/building_materials")
            .WithTags("Building Materials");

        group.MapGet("/", GetAllBuildingMaterials)
            .WithName("GetBuildingMaterials")
            .WithSummary("Gets all building materials.");
        group.MapPost("/", CreateBuildingMaterial)
            .WithName("CreateBuildingMaterial")
            .WithSummary("Creates a building material.");
        group.MapDelete("/{name}", DeleteBuildingMaterial)
            .WithName("DeleteBuildingMaterial")
            .WithSummary("Deletes a building material by name.");

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
