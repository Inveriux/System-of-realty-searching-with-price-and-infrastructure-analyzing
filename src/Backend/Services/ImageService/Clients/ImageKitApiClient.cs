using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BasicBuildingBlocks.BusinessResults;
using ImageService.Clients.DTOs;
using ImageService.Configuration;
using Microsoft.Extensions.Options;

namespace ImageService.Clients;

public sealed class ImageKitApiClient(
    HttpClient httpClient,
    IOptions<ImageKitOptions> options,
    ILogger<ImageKitApiClient> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly ImageKitOptions _options = options.Value;
    private readonly ILogger<ImageKitApiClient> _logger = logger;

    public Task<Result<ImageKitUploadResponseDto>> UploadFileAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder,
        bool useUniqueFileName,
        CancellationToken cancellationToken = default)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        content.Add(fileContent, "file", fileName);
        AddCommonFields(content, fileName, folder, useUniqueFileName);

        return SendUploadAsync(content, fileName, cancellationToken);
    }

    public Task<Result<ImageKitUploadResponseDto>> UploadUrlAsync(
        string imageUrl,
        string fileName,
        string folder,
        bool useUniqueFileName,
        CancellationToken cancellationToken = default)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(imageUrl), "file");
        AddCommonFields(content, fileName, folder, useUniqueFileName);

        return SendUploadAsync(content, fileName, cancellationToken);
    }

    private void AddCommonFields(
        MultipartFormDataContent content,
        string fileName,
        string folder,
        bool useUniqueFileName)
    {
        content.Add(new StringContent(fileName), "fileName");
        content.Add(new StringContent(folder), "folder");
        content.Add(new StringContent(useUniqueFileName ? "true" : "false"), "useUniqueFileName");
    }

    private async Task<Result<ImageKitUploadResponseDto>> SendUploadAsync(
        MultipartFormDataContent content,
        string fileName,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.UploadEndpoint)
        {
            Content = content
        };

        request.Headers.Authorization = CreateAuthorizationHeader(_options.PrivateKey);

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return HandleErrorResponse(response.StatusCode, body, fileName);
            }

            var payload = JsonSerializer.Deserialize<ImageKitUploadResponseDto>(body, JsonOptions);
            if (payload is null || string.IsNullOrWhiteSpace(payload.FileId) || string.IsNullOrWhiteSpace(payload.Url))
            {
                _logger.LogError("ImageKit returned an unreadable success payload for file {FileName}.", fileName);
                return Result.Fail<ImageKitUploadResponseDto>(
                    ResultStatusCode.Fail,
                    "Не удалось загрузить изображение во внешнее хранилище.");
            }

            _logger.LogInformation(
                "Image uploaded to ImageKit. FileName: {FileName}, FileId: {FileId}, Url: {Url}",
                fileName,
                payload.FileId,
                payload.Url);

            return Result.Ok(payload);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("ImageKit upload timed out for file {FileName}.", fileName);
            return Result.Fail<ImageKitUploadResponseDto>(
                ResultStatusCode.Fail,
                "Превышено время ожидания загрузки изображения.");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "ImageKit upload request failed for file {FileName}.", fileName);
            return Result.Fail<ImageKitUploadResponseDto>(
                ResultStatusCode.Fail,
                "Не удалось загрузить изображение во внешнее хранилище.");
        }
    }

    private Result<ImageKitUploadResponseDto> HandleErrorResponse(
        HttpStatusCode statusCode,
        string body,
        string fileName)
    {
        string? providerMessage = null;

        try
        {
            providerMessage = JsonSerializer.Deserialize<ImageKitErrorResponseDto>(body, JsonOptions)?.Message;
        }
        catch (JsonException)
        {
        }

        _logger.LogWarning(
            "ImageKit upload failed. StatusCode: {StatusCode}, FileName: {FileName}, ProviderMessage: {ProviderMessage}",
            (int)statusCode,
            fileName,
            providerMessage ?? body);

        var message = statusCode switch
        {
            HttpStatusCode.BadRequest => "Не удалось загрузить изображение во внешнее хранилище.",
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "Ошибка авторизации во внешнем хранилище изображений.",
            HttpStatusCode.RequestEntityTooLarge => "Изображение слишком большое для внешнего хранилища.",
            _ => "Не удалось загрузить изображение во внешнее хранилище."
        };

        return Result.Fail<ImageKitUploadResponseDto>(ResultStatusCode.Fail, message);
    }

    private static AuthenticationHeaderValue CreateAuthorizationHeader(string privateKey)
    {
        var value = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{privateKey}:"));
        return new AuthenticationHeaderValue("Basic", value);
    }
}
