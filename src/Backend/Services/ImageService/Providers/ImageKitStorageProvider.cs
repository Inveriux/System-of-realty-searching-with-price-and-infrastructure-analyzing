using BasicBuildingBlocks.BusinessResults;
using ImageService.Clients;
using ImageService.Configuration;
using Microsoft.Extensions.Options;

namespace ImageService.Providers;

public sealed class ImageKitStorageProvider(
    ImageKitApiClient imageKitApiClient,
    IOptions<ImageKitOptions> options) : IImageStorageProvider
{
    private readonly ImageKitApiClient _imageKitApiClient = imageKitApiClient;
    private readonly ImageKitOptions _options = options.Value;

    public async Task<Result<StoredImageResult>> UploadAsync(
        UploadImageCommand command,
        CancellationToken cancellationToken = default)
    {
        var response = await _imageKitApiClient.UploadFileAsync(
            command.Stream,
            command.FileName,
            command.ContentType,
            command.Folder,
            _options.UseUniqueFileName,
            cancellationToken);

        if (!response.IsSuccess || response.Data is null)
            return Result.Fail<StoredImageResult>(response.StatusCode, response.Message, response.Errors);

        return Result.Ok(Map(response.Data, command.ContentType));
    }

    public async Task<Result<StoredImageResult>> UploadAsync(
        string sourceUrl,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default)
    {
        var response = await _imageKitApiClient.UploadUrlAsync(
            sourceUrl,
            fileName,
            folder,
            _options.UseUniqueFileName,
            cancellationToken);

        if (!response.IsSuccess || response.Data is null)
            return Result.Fail<StoredImageResult>(response.StatusCode, response.Message, response.Errors);

        return Result.Ok(Map(response.Data, contentType));
    }

    private static StoredImageResult Map(Clients.DTOs.ImageKitUploadResponseDto response, string contentType)
        => new()
        {
            Provider = "ImageKit",
            ProviderFileId = response.FileId,
            FileName = response.Name,
            Url = response.Url,
            ThumbnailUrl = response.ThumbnailUrl,
            FilePath = response.FilePath,
            ContentType = !string.IsNullOrWhiteSpace(response.FileType) && response.FileType.Contains('/')
                ? response.FileType
                : contentType,
            Size = response.Size,
            Width = response.Width,
            Height = response.Height
        };
}
