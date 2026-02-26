using ListingService.Application.UseCases.ClassTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;

public class ClassTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/class_types");

        group.MapGet("/", GetAllClassTypes);
        group.MapPost("/", CreateClassType);
        group.MapDelete("/{name}", DeleteClassType);

        return group;
    }

    public async Task<IResult> GetAllClassTypes(ISender sender)
    {
        var result = await sender.Send(new GetClassTypesQuery());
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateClassType(ISender sender,
        LookupNameDto dto)
    {
        var result = await sender.Send(new CreateClassTypeCommand(dto.Name));
        return result.ToHttpResult();
    }

    public async Task<IResult> DeleteClassType(ISender sender,
        [FromRoute] string name)
    {
        var result = await sender.Send(new DeleteClassTypeCommand(name));
        return result.ToHttpResult();
    }
}
