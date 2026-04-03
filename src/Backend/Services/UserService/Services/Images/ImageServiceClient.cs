using System.Net.Http.Headers;
using System.Text.Json;
using BasicBuildingBlocks.BusinessResults;
using Microsoft.Extensions.Options;
using UserService.Services.Images.Abstractions;

namespace UserService.Services.Images;

public sealed class ImageServiceClient(HttpClient httpClient, IOptions<ImageServiceOptions> options) : IImageServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly ImageServiceOptions _options = options.Value;

    public async Task<Result<UploadedImageDto>> UploadSingleAsync(IFormFile file, CancellationToken cancellationToken)
    {
        using var formData = new MultipartFormDataContent();
        var stream = file.OpenReadStream();
        var content = new StreamContent(stream);
        if (!string.IsNullOrWhiteSpace(file.ContentType))
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

        formData.Add(content, "image", file.FileName);

        using var response = await _httpClient.PostAsync($"{_options.BaseUrl}/api/images/upload", formData, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        ImageServiceApiResult<UploadedImageDto>? result = null;
        try
        {
            result = JsonSerializer.Deserialize<ImageServiceApiResult<UploadedImageDto>>(payload, JsonOptions);
        }
        catch (JsonException)
        {
        }

        if (!response.IsSuccessStatusCode || result?.IsSuccess != true || result.Data is null)
            return Result.Fail<UploadedImageDto>(result?.Message ?? payload ?? "Ошибка запроса к сервису изображений.");

        return Result.Ok(result.Data, result.Message);
    }
}
