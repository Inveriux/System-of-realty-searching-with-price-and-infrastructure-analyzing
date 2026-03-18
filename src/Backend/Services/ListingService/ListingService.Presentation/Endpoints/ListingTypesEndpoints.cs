using ListingService.Application.UseCases.ListingTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BasicBuildingBlocks.BusinessResults.Extensoins;

public class ListingTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/listing_types")
            .WithTags("Listing Types");

        group.MapGet("/", GetAllListingTypes)
            .WithName("GetListingTypes")
            .WithSummary("Gets all listing types.");
        group.MapPost("/", CreateListingType)
            .WithName("CreateListingType")
            .WithSummary("Creates a listing type.");
        group.MapDelete("/{name}", DeleteListingType)
            .WithName("DeleteListingType")
            .WithSummary("Deletes a listing type by name.");

        return group;
    }

    public async Task<IResult> GetAllListingTypes(ISender sender)
    {
        var result = await sender.Send(new GetListingTypesQuery());
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateListingType(ISender sender,
        LookupNameDto dto)
    {
        var result = await sender.Send(new CreateListingTypeCommand(dto.Name));
        return result.ToHttpResult();
    }

    public async Task<IResult> DeleteListingType(ISender sender,
        [FromRoute] string name)
    {
        var result = await sender.Send(new DeleteListingTypeCommand(name));
        return result.ToHttpResult();
    }
}
