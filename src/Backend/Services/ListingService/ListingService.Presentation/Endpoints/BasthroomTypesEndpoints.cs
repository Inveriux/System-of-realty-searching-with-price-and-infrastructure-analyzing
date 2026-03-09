using ListingService.Application.UseCases.BathroomTypes;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;

public class BathroomTypesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup($"/api/bathroom_types");

        group.MapGet("/", GetAllBathroomTypes);
        group.MapPost("/", CreateBathroomType);
        group.MapDelete("/{name}", DeleteBathroomType);

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