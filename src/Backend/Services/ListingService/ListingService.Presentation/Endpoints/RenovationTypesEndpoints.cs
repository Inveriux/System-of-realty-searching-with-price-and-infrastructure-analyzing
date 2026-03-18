using ListingService.Application.UseCases.RenovationTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BasicBuildingBlocks.BusinessResults.Extensoins;

public class RenovationTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/renovation_types")
            .WithTags("Renovation Types");

        group.MapGet("/", GetAllRenovationTypes)
            .WithName("GetRenovationTypes")
            .WithSummary("Gets all renovation types.");
        group.MapPost("/", CreateRenovationType)
            .WithName("CreateRenovationType")
            .WithSummary("Creates a renovation type.");
        group.MapDelete("/{name}", DeleteRenovationType)
            .WithName("DeleteRenovationType")
            .WithSummary("Deletes a renovation type by name.");

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
