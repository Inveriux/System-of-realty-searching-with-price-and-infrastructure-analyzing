using System.Net.Http.Headers;
using System.Text.Json;
using BasicBuildingBlocks.BusinessResults;
using ListingService.Presentation.Dtos.Images;
using ListingService.Presentation.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace ListingService.Presentation.Services;

public sealed class ImageServiceClient(HttpClient httpClient, IOptions<ImageServiceOptions> options) : IImageServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly ImageServiceOptions _options = options.Value;

    public Task<Result<UploadedImageDto>> UploadSingleAsync(IFormFile file, CancellationToken cancellationToken)
        => SendSingleAsync(file, cancellationToken);

    public Task<Result<IReadOnlyCollection<UploadedImageDto>>> UploadManyAsync(IEnumerable<IFormFile> files, CancellationToken cancellationToken)
        => SendManyAsync(files, cancellationToken);

    private async Task<Result<UploadedImageDto>> SendSingleAsync(IFormFile file, CancellationToken cancellationToken)
    {
        using var formData = new MultipartFormDataContent();
        await AddFileAsync(formData, "image", file, cancellationToken);

        using var response = await _httpClient.PostAsync($"{_options.BaseUrl}/api/images/upload", formData, cancellationToken);
        return await ParseResponseAsync<UploadedImageDto>(response, cancellationToken);
    }

    private async Task<Result<IReadOnlyCollection<UploadedImageDto>>> SendManyAsync(IEnumerable<IFormFile> files, CancellationToken cancellationToken)
    {
        using var formData = new MultipartFormDataContent();
        foreach (var file in files)
            await AddFileAsync(formData, "images", file, cancellationToken);

        using var response = await _httpClient.PostAsync($"{_options.BaseUrl}/api/images/upload-many", formData, cancellationToken);
        return await ParseResponseAsync<IReadOnlyCollection<UploadedImageDto>>(response, cancellationToken);
    }

    private static async Task AddFileAsync(MultipartFormDataContent formData, string fieldName, IFormFile file, CancellationToken cancellationToken)
    {
        var stream = file.OpenReadStream();
        var content = new StreamContent(stream);
        if (!string.IsNullOrWhiteSpace(file.ContentType))
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

        formData.Add(content, fieldName, file.FileName);
        await Task.CompletedTask;
    }

    private static async Task<Result<T>> ParseResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        ImageServiceApiResult<T>? result = null;
        try
        {
            result = JsonSerializer.Deserialize<ImageServiceApiResult<T>>(payload, JsonOptions);
        }
        catch (JsonException)
        {
        }

        if (!response.IsSuccessStatusCode || result?.IsSuccess != true || result.Data is null)
            return Result.Fail<T>(result?.Message ?? payload ?? "Image service request failed.");

        return Result.Ok(result.Data, result.Message);
    }
}
