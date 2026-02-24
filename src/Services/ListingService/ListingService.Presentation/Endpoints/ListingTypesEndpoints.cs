using ListingService.Application.UseCases.ListingTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;

public class ListingTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder RegisterEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/listing_types");

        group.MapGet("/", GetAllListingTypes);
        group.MapPost("/", CreateListingType);
        group.MapDelete("/{name}", DeleteListingType);

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
