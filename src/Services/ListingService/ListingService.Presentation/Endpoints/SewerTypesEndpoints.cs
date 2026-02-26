using ListingService.Application.UseCases.SewerTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;

public class SewerTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/sewer_types");

        group.MapGet("/", GetAllSewerTypes);
        group.MapPost("/", CreateSewerType);
        group.MapDelete("/{name}", DeleteSewerType);

        return group;
    }

    public async Task<IResult> GetAllSewerTypes(ISender sender)
    {
        var result = await sender.Send(new GetSewerTypesQuery());
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateSewerType(ISender sender,
        LookupNameDto dto)
    {
        var result = await sender.Send(new CreateSewerTypeCommand(dto.Name));
        return result.ToHttpResult();
    }

    public async Task<IResult> DeleteSewerType(ISender sender,
        [FromRoute] string name)
    {
        var result = await sender.Send(new DeleteSewerTypeCommand(name));
        return result.ToHttpResult();
    }
}
