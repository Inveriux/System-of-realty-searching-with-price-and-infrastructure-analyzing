using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using BasicBuildingBlocks.BusinessResults;
using ImageService.DTOs;
using ImageService.Models;
using ImageService.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace ImageService.Services;

public sealed class ImageBbService(HttpClient httpClient, IOptions<ImageBbOptions> options) : IImageBbService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly ImageBbOptions _options = options.Value;

    public async Task<Result<ImageUploadResponseDto>> UploadFileAsync(
        IFormFile file,
        string? name,
        int? expiration,
        CancellationToken cancellationToken)
    {
        var validationResult = ValidateFile(file);
        if (!validationResult.IsSuccess)
            return Result.Fail<ImageUploadResponseDto>(validationResult.StatusCode, validationResult.Message, validationResult.Errors);

        using var formData = new MultipartFormDataContent();
        AddCommonFields(formData, name, expiration);

        await using var stream = file.OpenReadStream();
        using var imageContent = new StreamContent(stream);
        if (!string.IsNullOrWhiteSpace(file.ContentType))
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

        formData.Add(imageContent, "image", file.FileName);

        return await SendAsync(formData, cancellationToken);
    }

    public async Task<Result<ImageUploadResponseDto>> UploadUrlAsync(
        string imageUrl,
        string? name,
        int? expiration,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
            return Result.Validation<ImageUploadResponseDto>("`imageUrl` must be a valid absolute URL.");

        if (expiration is not null && (expiration < 60 || expiration > 15552000))
            return Result.Validation<ImageUploadResponseDto>("`expiration` must be between 60 and 15552000 seconds.");

        using var formData = new MultipartFormDataContent();
        AddCommonFields(formData, name, expiration);
        formData.Add(new StringContent(imageUrl), "image");

        return await SendAsync(formData, cancellationToken);
    }

    private Result ValidateFile(IFormFile? file)
    {
        if (file is null)
            return Result.Validation("Image file is required.");

        if (file.Length <= 0)
            return Result.Validation("Image file must not be empty.");

        if (file.Length > _options.MaxFileSizeInBytes)
            return Result.Validation($"Image file exceeds the size limit of {_options.MaxFileSizeInBytes} bytes.");

        return Result.Ok();
    }

    private void AddCommonFields(MultipartFormDataContent formData, string? name, int? expiration)
    {
        if (expiration is not null && (expiration < 60 || expiration > 15552000))
            throw new ArgumentOutOfRangeException(nameof(expiration), "`expiration` must be between 60 and 15552000 seconds.");

        formData.Add(new StringContent(_options.ApiKey), "key");

        if (!string.IsNullOrWhiteSpace(name))
            formData.Add(new StringContent(name), "name");

        if (expiration is not null)
            formData.Add(new StringContent(expiration.Value.ToString()), "expiration");
    }

    private async Task<Result<ImageUploadResponseDto>> SendAsync(
        MultipartFormDataContent formData,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsync(_options.Endpoint, formData, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        ImageBbApiResponse? parsedResponse = null;
        try
        {
            parsedResponse = JsonSerializer.Deserialize<ImageBbApiResponse>(payload, JsonOptions);
        }
        catch (JsonException)
        {
        }

        if (!response.IsSuccessStatusCode || parsedResponse?.Success != true || parsedResponse.Data is null)
        {
            var errorMessage = parsedResponse?.Error?.Message
                ?? payload
                ?? "ImageBB upload failed.";

            return Result.Fail<ImageUploadResponseDto>(ResultStatusCode.Fail, errorMessage);
        }

        var data = parsedResponse.Data;
        return Result.Ok(new ImageUploadResponseDto(
            data.Id,
            data.Url,
            data.UrlViewer,
            data.DisplayUrl,
            data.DeleteUrl,
            data.Title,
            data.Image?.Filename,
            data.Image?.Url,
            data.Image?.Mime,
            data.Image?.Extension,
            data.Size,
            data.Time,
            data.Width,
            data.Height,
            data.Expiration,
            data.Thumb?.Url,
            data.Medium?.Url));
    }
}
