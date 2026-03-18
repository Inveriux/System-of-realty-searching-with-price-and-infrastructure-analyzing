using ListingService.Application.UseCases.HeatingTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BasicBuildingBlocks.BusinessResults.Extensoins;

public class HeatingTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/heating_types")
            .WithTags("Heating Types");

        group.MapGet("/", GetAllHeatingTypes)
            .WithName("GetHeatingTypes")
            .WithSummary("Gets all heating types.");
        group.MapPost("/", CreateHeatingType)
            .WithName("CreateHeatingType")
            .WithSummary("Creates a heating type.");
        group.MapDelete("/{name}", DeleteHeatingType)
            .WithName("DeleteHeatingType")
            .WithSummary("Deletes a heating type by name.");

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
