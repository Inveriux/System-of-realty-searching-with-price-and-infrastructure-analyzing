using System.Text.Json.Serialization;

namespace ImageService.Clients.DTOs;

public sealed class ImageKitErrorResponseDto
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("help")]
    public string? Help { get; set; }
}
