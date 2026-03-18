using ListingService.Application.UseCases.SewerTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BasicBuildingBlocks.BusinessResults.Extensoins;

public class SewerTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/sewer_types")
            .WithTags("Sewer Types");

        group.MapGet("/", GetAllSewerTypes)
            .WithName("GetSewerTypes")
            .WithSummary("Gets all sewer types.");
        group.MapPost("/", CreateSewerType)
            .WithName("CreateSewerType")
            .WithSummary("Creates a sewer type.");
        group.MapDelete("/{name}", DeleteSewerType)
            .WithName("DeleteSewerType")
            .WithSummary("Deletes a sewer type by name.");

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
