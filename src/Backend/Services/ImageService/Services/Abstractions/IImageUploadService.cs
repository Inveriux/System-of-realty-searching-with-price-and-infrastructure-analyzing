using BasicBuildingBlocks.BusinessResults;
using ImageService.DTOs;

namespace ImageService.Services.Abstractions;

public interface IImageUploadService
{
    Task<Result<ImageUploadResponseDto>> UploadFileAsync(
        IFormFile file,
        string? name,
        int? expiration,
        CancellationToken cancellationToken);

    Task<Result<ImageUploadResponseDto>> UploadUrlAsync(
        string imageUrl,
        string? name,
        int? expiration,
        CancellationToken cancellationToken);
}
