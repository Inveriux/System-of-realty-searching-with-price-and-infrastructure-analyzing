using ListingService.Application.UseCases.LayoutTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;

public class LayoutTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/layout_types");

        group.MapGet("/", GetAllLayoutTypes);
        group.MapPost("/", CreateLayoutType);
        group.MapDelete("/{name}", DeleteLayoutType);

        return group;
    }

    public async Task<IResult> GetAllLayoutTypes(ISender sender)
    {
        var result = await sender.Send(new GetLayoutTypesQuery());
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateLayoutType(ISender sender,
        LookupNameDto dto)
    {
        var result = await sender.Send(new CreateLayoutTypeCommand(dto.Name));
        return result.ToHttpResult();
    }

    public async Task<IResult> DeleteLayoutType(ISender sender,
        [FromRoute] string name)
    {
        var result = await sender.Send(new DeleteLayoutTypeCommand(name));
        return result.ToHttpResult();
    }
}
