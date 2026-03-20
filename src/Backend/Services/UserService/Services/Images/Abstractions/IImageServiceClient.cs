using BasicBuildingBlocks.BusinessResults;

namespace UserService.Services.Images.Abstractions;

public interface IImageServiceClient
{
    Task<Result<UploadedImageDto>> UploadSingleAsync(IFormFile file, CancellationToken cancellationToken);
}
