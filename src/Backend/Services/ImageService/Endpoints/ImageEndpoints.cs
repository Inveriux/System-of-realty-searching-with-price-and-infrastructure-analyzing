using BasicBuildingBlocks.BusinessResults.Extensoins;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ImageService.DTOs;
using ImageService.UseCases;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ImageService.Endpoints;

public sealed class ImageEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var group = application.MapGroup("/api/images")
            .WithTags("Images");

        group.MapPost("/upload", UploadSingleAsync)
            .WithName("UploadSingleImage")
            .WithSummary("Uploads a single image file to external image storage.")
            .WithDescription("Accepts multipart/form-data with one file in the `image` field.")
            .Accepts<UploadImageRequest>("multipart/form-data")
            .DisableAntiforgery();

        group.MapPost("/upload-many", UploadManyAsync)
            .WithName("UploadManyImages")
            .WithSummary("Uploads up to 10 image files to external image storage.")
            .WithDescription("Accepts multipart/form-data with files in the `images` field.")
            .Accepts<UploadManyImagesRequest>("multipart/form-data")
            .DisableAntiforgery();

        group.MapPost("/upload-by-url", UploadByUrlAsync)
            .WithName("UploadImageByUrl")
            .WithSummary("Uploads an image to external image storage by public URL.")
            .WithDescription("Accepts JSON with `imageUrl` and optional `name` and `expiration`.");

        return group;
    }

    public async Task<IResult> UploadSingleAsync(
        ISender sender,
        [FromForm] UploadImageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UploadImageCommand(request), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> UploadManyAsync(
        ISender sender,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var form = await request.ReadFormAsync(cancellationToken);
        var images = form.Files.GetFiles("images");

        int? expiration = null;
        var expirationValue = form["expiration"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(expirationValue) &&
            int.TryParse(expirationValue, out var parsedExpiration))
        {
            expiration = parsedExpiration;
        }

        var payload = new UploadManyImagesRequest
        {
            Images = images.ToList(),
            Expiration = expiration
        };

        var result = await sender.Send(new UploadManyImagesCommand(payload), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> UploadByUrlAsync(
        ISender sender,
        UploadImageByUrlRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UploadImageByUrlCommand(request), cancellationToken);
        return result.ToHttpResult();
    }
}
