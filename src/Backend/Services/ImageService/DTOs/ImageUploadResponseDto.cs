namespace ImageService.DTOs;

public sealed record ImageUploadResponseDto(
    string Id,
    string Url,
    string? ViewerUrl,
    string? DisplayUrl,
    string? DeleteUrl,
    string? Title,
    string? FileName,
    string? FileUrl,
    string? MimeType,
    string? Extension,
    long? Size,
    long? Time,
    int? Width,
    int? Height,
    int? Expiration,
    string? ThumbUrl,
    string? MediumUrl);
