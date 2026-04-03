using ListingService.Presentation.Services.Abstractions;
using BasicBuildingBlocks.BusinessResults.Extensoins;
using ListingService.Application.UseCases.Listings;
using ListingService.Presentation.Converters.Json;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ListingService.Presentation.Dtos.Listings;
using ListingService.Application.DTOs.Listings;
using ListingService.Presentation.Filters;
using ListingService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using MediatR;

namespace ListingService.Presentation.Endpoints.Listings;

public class ListingsEndpoints : IEndpointRegistrar
{
    private static readonly JsonSerializerOptions ListingJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        ListingJsonOptions.Converters.Add(new CreateListingDtoJsonConverter());
        ListingJsonOptions.Converters.Add(new UpdateListingRequestDtoJsonConverter());

        var group = application.MapGroup("api/listings")
            .WithTags("Listings");

        group.MapGet("/", GetAllAsync)
            .WithName("GetListings")
            .WithSummary("Gets paged listings.")
            .WithDescription("Returns a paginated list of listings. Supports page and pageSize query parameters.");
        group.MapGet("/details", GetAllDetailedAsync)
            .WithName("GetDetailedListings")
            .WithSummary("Gets paged detailed listings.")
            .WithDescription("Returns a paginated list of listings with media, pricing and polymorphic listing details.");
        group.MapGet("/{id}", GetByIdAsync)
            .WithName("GetListingById")
            .WithSummary("Gets a listing by id.")
            .WithDescription("Returns the base listing projection without expanded listing details.");
        group.MapGet("/details/{id}", GetDetailedByIdAsync)
            .WithName("GetDetailedListingById")
            .WithSummary("Gets a listing with detailed payload.")
            .WithDescription("Returns the listing with media, price history and a polymorphic details object.");
        group.MapPost("/", CreateListingAsync)
            .RequireAuthorization()
            .WithName("CreateListing")
            .WithSummary("Creates a new listing.")
            .WithDescription("Accepts the common listing payload and a polymorphic listingDetails object selected by listingDetailsType.");
        group.MapPost("/with-images", CreateListingWithImagesAsync)
            .RequireAuthorization()
            .WithName("CreateListingWithImages")
            .WithSummary("Creates a new listing and uploads its photos through ImageService.")
            .WithDescription("Accepts multipart/form-data with `listing` JSON and up to 10 files in `images`.")
            .DisableAntiforgery();
        group.MapPost("/{id}/images", UploadListingImageAsync)
            .AddEndpointFilter<OwnerOrAdminAccessEndpointFilter>()
            .RequireAuthorization(PolicyNames.OwnerOrAdminAccess)
            .WithName("UploadListingImage")
            .WithSummary("Uploads a single photo to an existing listing.")
            .WithDescription("Allows only the listing owner or an admin to append one photo via ImageService.")
            .Accepts<UploadListingImageRequest>("multipart/form-data")
            .DisableAntiforgery();
        group.MapGet("/{id:guid}/price-history", GetPriceHistoryAsync)
            .WithName("GetListingPriceHistory")
            .WithSummary("Gets price history for a listing.")
            .WithDescription("Returns the ordered price history timeline and basic change metrics.");
        group.MapPatch("/{id:guid}/price", UpdatePriceAsync)
            .AddEndpointFilter<OwnerOrAdminAccessEndpointFilter>()
            .RequireAuthorization(PolicyNames.OwnerOrAdminAccess)
            .WithName("UpdateListingPrice")
            .WithSummary("Updates current price for a listing.")
            .WithDescription("Updates current listing price and appends a new price history entry.");
        group.MapPatch("/{listingId:guid}", UpdateAsync)
            .AddEndpointFilter<OwnerOrAdminAccessEndpointFilter>()
            .RequireAuthorization(PolicyNames.OwnerOrAdminAccess)
            .WithName("UpdateListing")
            .WithSummary("Updates listing parameters.")
            .WithDescription("Updates a full listing payload, including common fields, media urls and polymorphic listing details.");
        group.MapPost("/{id:guid}/price-history", AddPriceHistoryEntryAsync)
            .AddEndpointFilter<OwnerOrAdminAccessEndpointFilter>()
            .RequireAuthorization(PolicyNames.OwnerOrAdminAccess)
            .WithName("AddListingPriceHistoryEntry")
            .WithSummary("Adds a manual price history point.")
            .WithDescription("Adds a dated point to listing price history for analytics/testing.");
        group.MapPatch("/{id:guid}/price-history/{historyEntryId:guid}", UpdatePriceHistoryEntryAsync)
            .AddEndpointFilter<OwnerOrAdminAccessEndpointFilter>()
            .RequireAuthorization(PolicyNames.OwnerOrAdminAccess)
            .WithName("UpdateListingPriceHistoryEntry")
            .WithSummary("Updates a manual price history point.")
            .WithDescription("Edits an existing price history point.");
        group.MapDelete("/{id:guid}/price-history/{historyEntryId:guid}", DeletePriceHistoryEntryAsync)
            .AddEndpointFilter<OwnerOrAdminAccessEndpointFilter>()
            .RequireAuthorization(PolicyNames.OwnerOrAdminAccess)
            .WithName("DeleteListingPriceHistoryEntry")
            .WithSummary("Deletes a price history point.")
            .WithDescription("Deletes one price history point while keeping the rest of timeline intact.");
        group.MapDelete("/{listingId:guid}", DeleteAsync)
            .AddEndpointFilter<OwnerOrAdminAccessEndpointFilter>()
            .RequireAuthorization(PolicyNames.OwnerOrAdminAccess)
            .WithName("DeleteListing")
            .WithSummary("Deletes a listing.")
            .WithDescription("Deletes a listing by id. Allowed only for the listing owner or an admin.");
        group.MapPatch("/{listingId:guid}/hide", HideAsync)
            .AddEndpointFilter<OwnerOrAdminAccessEndpointFilter>()
            .RequireAuthorization(PolicyNames.OwnerOrAdminAccess)
            .WithName("HideListing")
            .WithSummary("Hides a listing.")
            .WithDescription("Marks a listing as hidden and removes it from public search.");
        group.MapPatch("/{listingId:guid}/unhide", UnhideAsync)
            .AddEndpointFilter<OwnerOrAdminAccessEndpointFilter>()
            .RequireAuthorization(PolicyNames.OwnerOrAdminAccess)
            .WithName("UnhideListing")
            .WithSummary("Unhides a listing.")
            .WithDescription("Returns a hidden listing back to active search.");

        return group;
    }

    public async Task<IResult> GetAllAsync(ISender sender, 
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 0, 
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetListingQuery(page, pageSize), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> GetByIdAsync(ISender sender, 
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetListingByIdQuery(id), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> GetDetailedByIdAsync(ISender sender, 
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDetailedListingByIdQuery(id), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> GetAllDetailedAsync(
        ISender sender,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetDetailedListingsQuery(page, pageSize), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateListingAsync(ISender sender, 
        [FromBody] CreateListingCommand listing,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(listing, cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> UpdateAsync(
        ISender sender,
        [FromRoute] Guid listingId,
        [FromBody] UpdateListingRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateListingCommand(listingId, request), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> CreateListingWithImagesAsync(
        ISender sender,
        IImageServiceClient imageServiceClient,
        ILogger<ListingsEndpoints> logger,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var form = await request.ReadFormAsync(cancellationToken);
        var listing = form["listing"].FirstOrDefault();
        var images = form.Files.GetFiles("images");

        if (string.IsNullOrWhiteSpace(listing))
            return Results.BadRequest("Необходимо передать JSON-поле `listing`.");

        CreateListingDto? listingDto;
        try
        {
            listingDto = JsonSerializer.Deserialize<CreateListingDto>(listing, ListingJsonOptions);
        }
        catch (JsonException ex)
        {
            return Results.BadRequest($"Invalid listing json payload: {ex.Message}");
        }

        if (listingDto is null)
            return Results.BadRequest("Не удалось разобрать данные объявления.");

        var imagesCount = images.Count;
        logger.LogInformation(
            "CreateListingWithImages started. Bound images count: {ImagesCount}. Title: {Title}. ListingType: {ListingType}. DetailsType: {DetailsType}",
            imagesCount,
            listingDto.Title,
            listingDto.ListingType,
            listingDto.ListingDetailsType);

        if (images.Count > 0)
        {
            logger.LogInformation(
                "Bound image file names: {FileNames}",
                string.Join(", ", images.Select(static image => image.FileName)));

            var uploadResult = await imageServiceClient.UploadManyAsync(images, cancellationToken);

            logger.LogInformation(
                "ImageService upload finished. Success: {IsSuccess}. Returned item count: {ReturnedCount}. Message: {Message}",
                uploadResult.IsSuccess,
                uploadResult.Data?.Count ?? 0,
                uploadResult.Message);

            if (!uploadResult.IsSuccess || uploadResult.Data is null)
                return uploadResult.ToHttpResult();

            listingDto.MediaUrls = uploadResult.Data
                .Select(static image => image.Url)
                .Where(static url => !string.IsNullOrWhiteSpace(url))
                .ToList();

            logger.LogInformation(
                "Mapped ImageService urls. MediaUrls count: {MediaUrlsCount}. Urls: {Urls}",
                listingDto.MediaUrls.Count,
                string.Join(", ", listingDto.MediaUrls));
        }
        else
        {
            logger.LogWarning("CreateListingWithImages called with no bound images.");
        }

        logger.LogInformation(
            "Sending CreateListingCommand with MediaUrls count: {MediaUrlsCount}",
            listingDto.MediaUrls?.Count ?? 0);

        var result = await sender.Send(new CreateListingCommand(listingDto), cancellationToken);
        logger.LogInformation(
            "CreateListingCommand finished. Success: {IsSuccess}. Message: {Message}",
            result.IsSuccess,
            result.Message);

        if (!result.IsSuccess && result.Errors.Count > 0)
        {
            logger.LogWarning(
                "CreateListingCommand validation/business errors: {Errors}",
                string.Join("; ", result.Errors.Select(static error =>
                    string.IsNullOrWhiteSpace(error.PropertyName)
                        ? error.ErrorMessage
                        : $"{error.PropertyName}: {error.ErrorMessage}")));
        }

        return result.ToHttpResult();
    }

    public async Task<IResult> UploadListingImageAsync(
        ISender sender,
        IImageServiceClient imageServiceClient,
        [FromRoute] Guid id,
        [FromForm] UploadListingImageRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Image is null)
            return Results.BadRequest("Файл изображения обязателен.");

        var uploadResult = await imageServiceClient.UploadSingleAsync(request.Image, cancellationToken);
        if (!uploadResult.IsSuccess || uploadResult.Data is null)
            return uploadResult.ToHttpResult();

        var result = await sender.Send(
            new AddListingMediaUrlsCommand(id, [uploadResult.Data.Url]),
            cancellationToken);

        return result.ToHttpResult();
    }

    public async Task<IResult> GetPriceHistoryAsync(
        ISender sender,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetListingPriceHistoryQuery(id), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> UpdatePriceAsync(
        ISender sender,
        [FromRoute] Guid id,
        [FromBody] UpdateListingPriceRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateListingPriceCommand(id, request), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> AddPriceHistoryEntryAsync(
        ISender sender,
        [FromRoute] Guid id,
        [FromBody] CreateListingPriceHistoryEntryRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddListingPriceHistoryEntryCommand(id, request), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> UpdatePriceHistoryEntryAsync(
        ISender sender,
        [FromRoute] Guid id,
        [FromRoute] Guid historyEntryId,
        [FromBody] UpdateListingPriceHistoryEntryRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateListingPriceHistoryEntryCommand(id, historyEntryId, request), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> DeletePriceHistoryEntryAsync(
        ISender sender,
        [FromRoute] Guid id,
        [FromRoute] Guid historyEntryId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteListingPriceHistoryEntryCommand(id, historyEntryId), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> DeleteAsync(ISender sender,
        [FromRoute] Guid listingId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteListingCommand(listingId), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> HideAsync(
        ISender sender,
        [FromRoute] Guid listingId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new HideListingCommand(listingId), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> UnhideAsync(
        ISender sender,
        [FromRoute] Guid listingId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UnhideListingCommand(listingId), cancellationToken);
        return result.ToHttpResult();
    }
}
