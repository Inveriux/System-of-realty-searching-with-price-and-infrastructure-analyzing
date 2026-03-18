using ListingService.Application.UseCases.BuildingTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BasicBuildingBlocks.BusinessResults.Extensoins;

public class BuildingTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/building_types")
            .WithTags("Building Types");

        group.MapGet("/", GetAllBuildingTypes)
            .WithName("GetBuildingTypes")
            .WithSummary("Gets all building types.");
        group.MapPost("/", CreateBuildingType)
            .WithName("CreateBuildingType")
            .WithSummary("Creates a building type.");
        group.MapDelete("/{name}", DeleteBuildingType)
            .WithName("DeleteBuildingType")
            .WithSummary("Deletes a building type by name.");

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
