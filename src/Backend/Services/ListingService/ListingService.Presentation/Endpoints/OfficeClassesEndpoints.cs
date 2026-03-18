using ListingService.Application.UseCases.OfficeClasses;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Application.DTOs.Lookups;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BasicBuildingBlocks.BusinessResults.Extensoins;

public class OfficeClassesEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var groupName = GetType().Name;

        var group = application.MapGroup("/api/office_classes")
            .WithTags("Office Classes");

        group.MapGet("/", GetAllOfficeClasses)
            .WithName("GetOfficeClasses")
            .WithSummary("Gets all office classes.");
        group.MapPost("/", CreateOfficeClass)
            .WithName("CreateOfficeClass")
            .WithSummary("Creates an office class.");
        group.MapDelete("/{name}", DeleteOfficeClass)
            .WithName("DeleteOfficeClass")
            .WithSummary("Deletes an office class by name.");

        return group;
    }

    public async Task<IResult> GetAllOfficeClasses(ISender sender)
    {
        var result = await sender.Send(new GetOfficeClassesQuery());
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateOfficeClass(ISender sender,
        LookupNameDto dto)
    {
        var result = await sender.Send(new CreateOfficeClassCommand(dto.Name));
        return result.ToHttpResult();
    }

    public async Task<IResult> DeleteOfficeClass(ISender sender,
        [FromRoute] string name)
    {
        var result = await sender.Send(new DeleteOfficeClassCommand(name));
        return result.ToHttpResult();
    }
}
