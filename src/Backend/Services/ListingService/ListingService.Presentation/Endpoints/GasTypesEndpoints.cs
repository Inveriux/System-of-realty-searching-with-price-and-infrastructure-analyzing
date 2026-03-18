using ListingService.Application.UseCases.GasTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BasicBuildingBlocks.BusinessResults.Extensoins;

public class GasTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/gas_types")
            .WithTags("Gas Types");

        group.MapGet("/", GetAllGasTypes)
            .WithName("GetGasTypes")
            .WithSummary("Gets all gas types.");
        group.MapPost("/", CreateGasType)
            .WithName("CreateGasType")
            .WithSummary("Creates a gas type.");
        group.MapDelete("/{name}", DeleteGasType)
            .WithName("DeleteGasType")
            .WithSummary("Deletes a gas type by name.");

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
