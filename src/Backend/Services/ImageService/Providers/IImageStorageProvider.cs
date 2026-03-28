using BasicBuildingBlocks.BusinessResults;

namespace ImageService.Providers;

public interface IImageStorageProvider
{
    Task<Result<StoredImageResult>> UploadAsync(
        UploadImageCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<StoredImageResult>> UploadAsync(
        string sourceUrl,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default);
}
