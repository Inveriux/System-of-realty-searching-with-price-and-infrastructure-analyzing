using BasicBuildingBlocks.BusinessResults;
using BasicBuildingBlocks.Services.Abstractions;
using ChatService.Data;
using ChatService.DTOs.Chat;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.UseCases.Chat;

public sealed record GetUnreadCountQuery() : IRequest<Result<UnreadCountDto>>;

public sealed class GetUnreadCountHandler(
    ApplicationDbContext dbContext,
    ICurrentUserProvider currentUserProvider) : IRequestHandler<GetUnreadCountQuery, Result<UnreadCountDto>>
{
    public async Task<Result<UnreadCountDto>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserProvider.GetUserId();
        var count = await dbContext.Messages
            .AsNoTracking()
            .Where(x =>
                x.SenderUserId != currentUserId &&
                !x.IsRead &&
                x.Conversation.Participants.Any(p => p.UserId == currentUserId))
            .CountAsync(cancellationToken);

        return Result.Ok(new UnreadCountDto { Count = count });
    }
}
