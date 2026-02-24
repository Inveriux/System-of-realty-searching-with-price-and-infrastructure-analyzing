using ListingService.Application.UseCases.LandCategories;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;

public class LandCategoriesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder RegisterEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/land_categories");

        group.MapGet("/", GetAllLandCategories);
        group.MapPost("/", CreateLandCategory);
        group.MapDelete("/{name}", DeleteLandCategory);

        return group;
    }

    public async Task<IResult> GetAllLandCategories(ISender sender)
    {
        var result = await sender.Send(new GetLandCategoriesQuery());
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateLandCategory(ISender sender,
        LookupNameDto dto)
    {
        var result = await sender.Send(new CreateLandCategoryCommand(dto.Name));
        return result.ToHttpResult();
    }

    public async Task<IResult> DeleteLandCategory(ISender sender,
        [FromRoute] string name)
    {
        var result = await sender.Send(new DeleteLandCategoryCommand(name));
        return result.ToHttpResult();
    }
}
