using ListingService.Application.UseCases.HeatingTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;

public class HeatingTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/heating_types");

        group.MapGet("/", GetAllHeatingTypes);
        group.MapPost("/", CreateHeatingType);
        group.MapDelete("/{name}", DeleteHeatingType);

        return group;
    }

    public async Task<IResult> GetAllHeatingTypes(ISender sender)
    {
        var result = await sender.Send(new GetHeatingTypesQuery());
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateHeatingType(ISender sender,
        LookupNameDto dto)
    {
        var result = await sender.Send(new CreateHeatingTypeCommand(dto.Name));
        return result.ToHttpResult();
    }

    public async Task<IResult> DeleteHeatingType(ISender sender,
        [FromRoute] string name)
    {
        var result = await sender.Send(new DeleteHeatingTypeCommand(name));
        return result.ToHttpResult();
    }
}
