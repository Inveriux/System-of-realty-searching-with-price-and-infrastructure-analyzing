using ListingService.Application.UseCases.LandCategories;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BasicBuildingBlocks.BusinessResults.Extensoins;

public class LandCategoriesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/land_categories")
            .WithTags("Land Categories");

        group.MapGet("/", GetAllLandCategories)
            .WithName("GetLandCategories")
            .WithSummary("Gets all land categories.");
        group.MapPost("/", CreateLandCategory)
            .WithName("CreateLandCategory")
            .WithSummary("Creates a land category.");
        group.MapDelete("/{name}", DeleteLandCategory)
            .WithName("DeleteLandCategory")
            .WithSummary("Deletes a land category by name.");

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
