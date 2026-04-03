using BasicBuildingBlocks.BusinessResults;
using ImageService.Configuration;
using ImageService.DTOs;
using ImageService.Services.Abstractions;
using MediatR;
using Microsoft.Extensions.Options;

namespace ImageService.UseCases;

public sealed record UploadManyImagesCommand(UploadManyImagesRequest Request) : IRequest<Result<IReadOnlyCollection<ImageUploadResponseDto>>>;

public sealed class UploadManyImagesCommandHandler(
    IImageUploadService imageUploadService,
    IOptions<ImageKitOptions> options)
    : IRequestHandler<UploadManyImagesCommand, Result<IReadOnlyCollection<ImageUploadResponseDto>>>
{
    private readonly ImageKitOptions _options = options.Value;

    public async Task<Result<IReadOnlyCollection<ImageUploadResponseDto>>> Handle(
        UploadManyImagesCommand request,
        CancellationToken cancellationToken)
    {
        var images = request.Request.Images ?? [];

        if (request.Request.Expiration is not null &&
            (request.Request.Expiration < 60 || request.Request.Expiration > 15552000))
        {
            return Result.Validation<IReadOnlyCollection<ImageUploadResponseDto>>("`expiration` должен быть в диапазоне от 60 до 15552000 секунд.");
        }

        if (images.Count == 0)
            return Result.Validation<IReadOnlyCollection<ImageUploadResponseDto>>("Необходимо передать хотя бы одно изображение.");

        if (images.Count > _options.MaxFilesPerRequest)
        {
            return Result.Validation<IReadOnlyCollection<ImageUploadResponseDto>>(
                $"За один запрос можно загрузить не более {_options.MaxFilesPerRequest} изображений.");
        }

        var uploadedImages = new List<ImageUploadResponseDto>(images.Count);

        foreach (var image in images)
        {
            var result = await imageUploadService.UploadFileAsync(
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
