using BasicBuildingBlocks.BusinessResults;
using ImageService.DTOs;
using ImageService.Services.Abstractions;
using MediatR;

namespace ImageService.UseCases;

public sealed record UploadImageCommand(UploadImageRequest Request) : IRequest<Result<ImageUploadResponseDto>>;

public sealed class UploadImageCommandHandler(IImageUploadService imageUploadService)
    : IRequestHandler<UploadImageCommand, Result<ImageUploadResponseDto>>
{
    public async Task<Result<ImageUploadResponseDto>> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        if (request.Request.Expiration is not null &&
            (request.Request.Expiration < 60 || request.Request.Expiration > 15552000))
        {
            return Result.Validation<ImageUploadResponseDto>("`expiration` должен быть в диапазоне от 60 до 15552000 секунд.");
        }

        if (request.Request.Image is null)
            return Result.Validation<ImageUploadResponseDto>("Файл изображения обязателен.");

        return await imageUploadService.UploadFileAsync(
            request.Request.Image,
            request.Request.Name,
            request.Request.Expiration,
            cancellationToken);
    }
}
