using BasicBuildingBlocks.Authorization;
using BasicBuildingBlocks.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using ListingService.Application.Contracts.Repositories;
using ListingService.Domain.Enums;

namespace ListingService.Presentation.Filters;

public class OwnerOrAdminAccessEndpointFilter
    : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var authorizationService = context.HttpContext
            .RequestServices
            .GetRequiredService<IAuthorizationService>();

        var repositoryManager = context.HttpContext
            .RequestServices
            .GetRequiredService<IRepositoryManager>();

        var routeValue = context.HttpContext.GetRouteValue("listingId")
            ?? context.HttpContext.GetRouteValue("id");

        if (!Guid.TryParse(routeValue?.ToString(), out var parsedListingId))
            return Results.BadRequest("Некорректный идентификатор объявления.");

        var listing = await repositoryManager.Listing.GetByIdAsync(parsedListingId, false, context.HttpContext.RequestAborted);
        if (listing is null)
            return Results.NotFound();

        var ownerId = listing.AgentId;
        var result = await authorizationService.AuthorizeAsync(
            context.HttpContext.User,
            ownerId,
            new OwnerOrAdminAccessAuthorizationRequirement(UserRoles.Admin));

        if (!result.Succeeded)
            return Results.Forbid();

        return await next(context);
    }
}
