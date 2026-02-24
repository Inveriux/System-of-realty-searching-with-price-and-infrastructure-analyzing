using ListingService.Application.UseCases.GasTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;

public class GasTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder RegisterEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/gas_types");

        group.MapGet("/", GetAllGasTypes);
        group.MapPost("/", CreateGasType);
        group.MapDelete("/{name}", DeleteGasType);

        return group;
    }

    public async Task<IResult> GetAllGasTypes(ISender sender)
    {
        var result = await sender.Send(new GetGasTypesQuery());
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateGasType(ISender sender,
        LookupNameDto dto)
    {
        var result = await sender.Send(new CreateGasTypeCommand(dto.Name));
        return result.ToHttpResult();
    }

    public async Task<IResult> DeleteGasType(ISender sender,
        [FromRoute] string name)
    {
        var result = await sender.Send(new DeleteGasTypeCommand(name));
        return result.ToHttpResult();
    }
}
