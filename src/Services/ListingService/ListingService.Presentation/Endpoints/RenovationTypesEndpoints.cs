using ListingService.Application.UseCases.RenovationTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;

public class RenovationTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder RegisterEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/renovation_types");

        group.MapGet("/", GetAllRenovationTypes);
        group.MapPost("/", CreateRenovationType);
        group.MapDelete("/{name}", DeleteRenovationType);

        return group;
    }

    public async Task<IResult> GetAllRenovationTypes(ISender sender)
    {
        var result = await sender.Send(new GetRenovationTypesQuery());
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateRenovationType(ISender sender,
        LookupNameDto dto)
    {
        var result = await sender.Send(new CreateRenovationTypeCommand(dto.Name));
        return result.ToHttpResult();
    }

    public async Task<IResult> DeleteRenovationType(ISender sender,
        [FromRoute] string name)
    {
        var result = await sender.Send(new DeleteRenovationTypeCommand(name));
        return result.ToHttpResult();
    }
}
