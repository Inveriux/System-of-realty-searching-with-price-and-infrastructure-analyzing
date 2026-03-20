using System.Text.Json.Serialization;

namespace ImageService.Models;

public sealed class ImageBbApiResponse
{
    [JsonPropertyName("data")]
    public ImageBbData? Data { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("error")]
    public ImageBbError? Error { get; set; }
}

public sealed class ImageBbData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url_viewer")]
    public string? UrlViewer { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("display_url")]
    public string? DisplayUrl { get; set; }

    [JsonPropertyName("delete_url")]
    public string? DeleteUrl { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("size")]
    public long? Size { get; set; }

    [JsonPropertyName("time")]
    public long? Time { get; set; }

    [JsonPropertyName("expiration")]
    public int? Expiration { get; set; }

    [JsonPropertyName("image")]
    public ImageBbFileVariant? Image { get; set; }

    [JsonPropertyName("thumb")]
    public ImageBbFileVariant? Thumb { get; set; }

    [JsonPropertyName("medium")]
    public ImageBbFileVariant? Medium { get; set; }
}

public sealed class ImageBbFileVariant
{
    [JsonPropertyName("filename")]
    public string? Filename { get; set; }

    [JsonPropertyName("mime")]
    public string? Mime { get; set; }

    [JsonPropertyName("extension")]
    public string? Extension { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public sealed class ImageBbError
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
