using ListingService.Application.UseCases.ClassTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BasicBuildingBlocks.BusinessResults.Extensoins;

public class ClassTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/class_types")
            .WithTags("Class Types");

        group.MapGet("/", GetAllClassTypes)
            .WithName("GetClassTypes")
            .WithSummary("Gets all class types.");
        group.MapPost("/", CreateClassType)
            .WithName("CreateClassType")
            .WithSummary("Creates a class type.");
        group.MapDelete("/{name}", DeleteClassType)
            .WithName("DeleteClassType")
            .WithSummary("Deletes a class type by name.");

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
