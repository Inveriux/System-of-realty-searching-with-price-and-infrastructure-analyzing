using ListingService.Application.UseCases.BathroomTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BasicBuildingBlocks.BusinessResults.Extensoins;

public class BathroomTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup($"/api/bathroom_types")
            .WithTags("Bathroom Types");

        group.MapGet("/", GetAllBathroomTypes)
            .WithName("GetBathroomTypes")
            .WithSummary("Gets all bathroom types.");
        group.MapPost("/", CreateBathroomType)
            .WithName("CreateBathroomType")
            .WithSummary("Creates a bathroom type.");
        group.MapDelete("/{name}", DeleteBathroomType)
            .WithName("DeleteBathroomType")
            .WithSummary("Deletes a bathroom type by name.");

        return group;
    }

    public async Task<IResult> GetAllBathroomTypes(ISender sender)
    {
        var result = await sender.Send(new GetBathroomTypesQuery());
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateBathroomType(ISender sender,
        LookupNameDto bathroomDto)
    {
        var result = await sender.Send(new CreateBathroomTypeCommand(bathroomDto.Name));
        return result.ToHttpResult();
    }

    public async Task<IResult> DeleteBathroomType(ISender sender,
        [FromRoute] string name)
    {
        var result = await sender.Send(new DeleteBathroomTypeCommand(name));
        return result.ToHttpResult();
    }
}
