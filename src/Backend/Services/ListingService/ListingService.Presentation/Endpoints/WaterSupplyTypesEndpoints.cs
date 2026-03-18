using ListingService.Application.UseCases.WaterSupplyTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BasicBuildingBlocks.BusinessResults.Extensoins;

public class WaterSupplyTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/water_supply_types")
            .WithTags("Water Supply Types");

        group.MapGet("/", GetAllWaterSupplyTypes)
            .WithName("GetWaterSupplyTypes")
            .WithSummary("Gets all water supply types.");
        group.MapPost("/", CreateWaterSupplyType)
            .WithName("CreateWaterSupplyType")
            .WithSummary("Creates a water supply type.");
        group.MapDelete("/{name}", DeleteWaterSupplyType)
            .WithName("DeleteWaterSupplyType")
            .WithSummary("Deletes a water supply type by name.");

        return group;
    }

    public async Task<IResult> GetAllWaterSupplyTypes(ISender sender)
    {
        var result = await sender.Send(new GetWaterSupplyTypesQuery());
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateWaterSupplyType(ISender sender,
        LookupNameDto dto)
    {
        var result = await sender.Send(new CreateWaterSupplyTypeCommand(dto.Name));
        return result.ToHttpResult();
    }

    public async Task<IResult> DeleteWaterSupplyType(ISender sender,
        [FromRoute] string name)
    {
        var result = await sender.Send(new DeleteWaterSupplyTypeCommand(name));
        return result.ToHttpResult();
    }
}
