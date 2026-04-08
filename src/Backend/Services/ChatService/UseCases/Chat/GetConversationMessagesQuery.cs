using BasicBuildingBlocks.BusinessResults;
using BasicBuildingBlocks.Services.Abstractions;
using ChatService.Data;
using ChatService.DTOs.Chat;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.UseCases.Chat;

public sealed record GetConversationMessagesQuery(Guid ConversationId, int Page, int PageSize)
    : IRequest<Result<ConversationMessagesPageDto>>;

public sealed class GetConversationMessagesHandler(
    ApplicationDbContext dbContext,
    ICurrentUserProvider currentUserProvider) : IRequestHandler<GetConversationMessagesQuery, Result<ConversationMessagesPageDto>>
{
    public async Task<Result<ConversationMessagesPageDto>> Handle(GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserProvider.GetUserId();
        var isParticipant = await dbContext.ConversationParticipants
            .AnyAsync(
                x => x.ConversationId == request.ConversationId && x.UserId == currentUserId,
                cancellationToken);

        if (!isParticipant)
        {
            return Result.NotFound<ConversationMessagesPageDto>("Диалог не найден.");
        }

        var skip = (request.Page - 1) * request.PageSize;
        var items = await dbContext.Messages
            .AsNoTracking()
            .Where(x => x.ConversationId == request.ConversationId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(request.PageSize + 1)
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > request.PageSize;
        var pageItems = items.Take(request.PageSize).OrderBy(x => x.CreatedAtUtc).Select(x => x.ToDto(currentUserId)).ToArray();

        return Result.Ok(new ConversationMessagesPageDto
        {
            Messages = pageItems,
            Page = request.Page,
            PageSize = request.PageSize,
            HasMore = hasMore
        });
    }
}
