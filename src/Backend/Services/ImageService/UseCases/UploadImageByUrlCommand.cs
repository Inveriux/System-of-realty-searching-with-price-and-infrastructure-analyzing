using BasicBuildingBlocks.BusinessResults;
using ImageService.DTOs;
using ImageService.Services.Abstractions;
using MediatR;

namespace ImageService.UseCases;

public sealed record UploadImageByUrlCommand(UploadImageByUrlRequest Request) : IRequest<Result<ImageUploadResponseDto>>;

public sealed class UploadImageByUrlCommandHandler(IImageBbService imageBbService)
    : IRequestHandler<UploadImageByUrlCommand, Result<ImageUploadResponseDto>>
{
    public Task<Result<ImageUploadResponseDto>> Handle(
        UploadImageByUrlCommand request,
        CancellationToken cancellationToken)
        => imageBbService.UploadUrlAsync(
            request.Request.ImageUrl,
            request.Request.Name,
            request.Request.Expiration,
            cancellationToken);
}
