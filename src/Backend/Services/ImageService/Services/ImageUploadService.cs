using System.Text;
using BasicBuildingBlocks.BusinessResults;
using ImageService.Configuration;
using ImageService.DTOs;
using ImageService.Providers;
using ImageService.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace ImageService.Services;

public sealed class ImageUploadService(
    IImageStorageProvider imageStorageProvider,
    IOptions<ImageKitOptions> options,
    ILogger<ImageUploadService> logger) : IImageUploadService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private readonly IImageStorageProvider _imageStorageProvider = imageStorageProvider;
    private readonly ImageKitOptions _options = options.Value;
    private readonly ILogger<ImageUploadService> _logger = logger;

    public async Task<Result<ImageUploadResponseDto>> UploadFileAsync(
        IFormFile file,
        string? name,
        int? expiration,
        CancellationToken cancellationToken)
    {
        var validationResult = await ValidateFileAsync(file, cancellationToken);
        if (!validationResult.IsSuccess)
            return Result.Fail<ImageUploadResponseDto>(validationResult.StatusCode, validationResult.Message, validationResult.Errors);

        if (expiration is not null)
        {
            _logger.LogInformation(
                "Image expiration was requested for file {FileName}, but ImageKit upload keeps files persistent. Requested expiration: {ExpirationSeconds}",
                file.FileName,
                expiration.Value);
        }

        var targetFileName = BuildSafeFileName(name, file.FileName);
        var folder = BuildFolder("uploads");

        await using var stream = file.OpenReadStream();
        var uploadResult = await _imageStorageProvider.UploadAsync(
            new UploadImageCommand
            {
                Stream = stream,
                FileName = targetFileName,
                ContentType = NormalizeContentType(file.ContentType, targetFileName),
                Folder = folder
            },
            cancellationToken);

        if (!uploadResult.IsSuccess || uploadResult.Data is null)
            return Result.Fail<ImageUploadResponseDto>(uploadResult.StatusCode, uploadResult.Message, uploadResult.Errors);

        return Result.Ok(Map(uploadResult.Data, expiration));
    }

    public async Task<Result<ImageUploadResponseDto>> UploadUrlAsync(
        string imageUrl,
        string? name,
        int? expiration,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var imageUri))
            return Result.Validation<ImageUploadResponseDto>("`imageUrl` must be a valid absolute URL.");

        if (expiration is not null)
        {
            _logger.LogInformation(
                "Image expiration was requested for URL upload {ImageUrl}, but ImageKit upload keeps files persistent. Requested expiration: {ExpirationSeconds}",
                imageUrl,
                expiration.Value);
        }

        var originalFileName = Path.GetFileName(imageUri.LocalPath);
        var targetFileName = BuildSafeFileName(name, originalFileName);
        var contentType = NormalizeContentType(null, targetFileName);

        var uploadResult = await _imageStorageProvider.UploadAsync(
            imageUrl,
            targetFileName,
            contentType,
            BuildFolder("uploads"),
            cancellationToken);

        if (!uploadResult.IsSuccess || uploadResult.Data is null)
            return Result.Fail<ImageUploadResponseDto>(uploadResult.StatusCode, uploadResult.Message, uploadResult.Errors);

        return Result.Ok(Map(uploadResult.Data, expiration));
    }

    private async Task<Result> ValidateFileAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
            return Result.Validation("Image file is required.");

        if (file.Length <= 0)
            return Result.Validation("Image file must not be empty.");

        if (file.Length > _options.MaxFileSizeInBytes)
        {
            return Result.Validation(
                $"Image file exceeds the size limit of {_options.MaxFileSizeInBytes} bytes.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            return Result.Validation("Unsupported image extension. Allowed: .jpg, .jpeg, .png, .webp.");

        if (!string.IsNullOrWhiteSpace(file.ContentType) && !AllowedContentTypes.Contains(file.ContentType))
            return Result.Validation("Unsupported image content type. Allowed: image/jpeg, image/png, image/webp.");

        await using var stream = file.OpenReadStream();
        var header = new byte[12];
        var read = await stream.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
        if (!IsSupportedImageSignature(header.AsSpan(0, read)))
            return Result.Validation("File signature does not match a supported image format.");

        return Result.Ok();
    }

    private static bool IsSupportedImageSignature(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 3 &&
            header[0] == 0xFF &&
            header[1] == 0xD8 &&
            header[2] == 0xFF)
        {
            return true;
        }

        if (header.Length >= 8 &&
            header[0] == 0x89 &&
            header[1] == 0x50 &&
            header[2] == 0x4E &&
            header[3] == 0x47 &&
            header[4] == 0x0D &&
            header[5] == 0x0A &&
            header[6] == 0x1A &&
            header[7] == 0x0A)
        {
            return true;
        }

        if (header.Length >= 12 &&
            header[0] == 0x52 &&
            header[1] == 0x49 &&
            header[2] == 0x46 &&
            header[3] == 0x46 &&
            header[8] == 0x57 &&
            header[9] == 0x45 &&
            header[10] == 0x42 &&
            header[11] == 0x50)
        {
            return true;
        }

        return false;
    }

    private string BuildFolder(string childFolder)
    {
        var normalizedBaseFolder = string.IsNullOrWhiteSpace(_options.BaseFolder)
            ? "/diploma"
            : _options.BaseFolder.Trim();

        if (!normalizedBaseFolder.StartsWith('/'))
            normalizedBaseFolder = $"/{normalizedBaseFolder}";

        normalizedBaseFolder = normalizedBaseFolder.TrimEnd('/');

        return $"{normalizedBaseFolder}/{childFolder.Trim('/')}";
    }

    private static string BuildSafeFileName(string? requestedName, string? originalFileName)
    {
        var sourceName = string.IsNullOrWhiteSpace(requestedName)
            ? originalFileName
            : requestedName;

        var extension = Path.GetExtension(sourceName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            extension = ".jpg";

        var baseName = Path.GetFileNameWithoutExtension(sourceName);
        if (string.IsNullOrWhiteSpace(baseName))
            baseName = "image";

        var builder = new StringBuilder(baseName.Length);
        foreach (var character in baseName.ToLowerInvariant())
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '-');
        }

        var normalizedBaseName = builder.ToString().Trim('-');
        if (string.IsNullOrWhiteSpace(normalizedBaseName))
            normalizedBaseName = "image";

        return $"{normalizedBaseName}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
    }

    private static string NormalizeContentType(string? contentType, string fileName)
    {
        if (!string.IsNullOrWhiteSpace(contentType) && AllowedContentTypes.Contains(contentType))
            return contentType;

        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
    }

    private static ImageUploadResponseDto Map(StoredImageResult result, int? expiration)
    {
        var extension = Path.GetExtension(result.FileName);

        return new ImageUploadResponseDto(
            result.ProviderFileId,
            result.Url,
            result.Url,
            result.Url,
            null,
            result.FileName,
            result.FileName,
            result.Url,
            result.ContentType,
            string.IsNullOrWhiteSpace(extension) ? null : extension.TrimStart('.'),
            result.Size,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            result.Width,
            result.Height,
            null,
            result.ThumbnailUrl,
            result.ThumbnailUrl);
    }
}
