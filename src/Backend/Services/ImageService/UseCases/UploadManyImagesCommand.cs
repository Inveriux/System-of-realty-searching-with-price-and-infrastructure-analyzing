using BasicBuildingBlocks.BusinessResults;
using ImageService.DTOs;
using ImageService.Services;
using ImageService.Services.Abstractions;
using MediatR;
using Microsoft.Extensions.Options;

namespace ImageService.UseCases;

public sealed record UploadManyImagesCommand(UploadManyImagesRequest Request) : IRequest<Result<IReadOnlyCollection<ImageUploadResponseDto>>>;

public sealed class UploadManyImagesCommandHandler(
    IImageBbService imageBbService,
    IOptions<ImageBbOptions> options)
    : IRequestHandler<UploadManyImagesCommand, Result<IReadOnlyCollection<ImageUploadResponseDto>>>
{
    private readonly ImageBbOptions _options = options.Value;

    public async Task<Result<IReadOnlyCollection<ImageUploadResponseDto>>> Handle(
        UploadManyImagesCommand request,
        CancellationToken cancellationToken)
    {
        var images = request.Request.Images ?? [];

        if (request.Request.Expiration is not null &&
            (request.Request.Expiration < 60 || request.Request.Expiration > 15552000))
        {
            return Result.Validation<IReadOnlyCollection<ImageUploadResponseDto>>("`expiration` must be between 60 and 15552000 seconds.");
        }

        if (images.Count == 0)
            return Result.Validation<IReadOnlyCollection<ImageUploadResponseDto>>("At least one image is required.");

        if (images.Count > _options.MaxFilesPerRequest)
        {
            return Result.Validation<IReadOnlyCollection<ImageUploadResponseDto>>(
                $"You can upload up to {_options.MaxFilesPerRequest} images per request.");
        }

        var uploadedImages = new List<ImageUploadResponseDto>(images.Count);

        foreach (var image in images)
        {
            var result = await imageBbService.UploadFileAsync(
                image,
                name: null,
                request.Request.Expiration,
                cancellationToken);

            if (!result.IsSuccess || result.Data is null)
                return Result.Fail<IReadOnlyCollection<ImageUploadResponseDto>>(result.StatusCode, result.Message, result.Errors);

            uploadedImages.Add(result.Data);
        }

        return Result.Ok<IReadOnlyCollection<ImageUploadResponseDto>>(uploadedImages);
    }
}
