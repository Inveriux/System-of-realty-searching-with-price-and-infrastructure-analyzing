using ListingService.Application.UseCases.WaterSupplyTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;

public class WaterSupplyTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder RegisterEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/water_supply_types");

        group.MapGet("/", GetAllWaterSupplyTypes);
        group.MapPost("/", CreateWaterSupplyType);
        group.MapDelete("/{name}", DeleteWaterSupplyType);

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
