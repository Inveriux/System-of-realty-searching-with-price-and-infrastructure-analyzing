using BasicBuildingBlocks.BusinessResults;
using ListingService.Presentation.Dtos.Images;

namespace ListingService.Presentation.Services.Abstractions;

public interface IImageServiceClient
{
    Task<Result<IReadOnlyCollection<UploadedImageDto>>> UploadManyAsync(IEnumerable<IFormFile> files, CancellationToken cancellationToken);
    Task<Result<UploadedImageDto>> UploadSingleAsync(IFormFile file, CancellationToken cancellationToken);
}
