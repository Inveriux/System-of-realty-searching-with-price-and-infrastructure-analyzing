using BasicBuildingBlocks.BusinessResults;
using BasicBuildingBlocks.Services.Abstractions;
using ChatService.Data;
using ChatService.DTOs.Chat;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.UseCases.Chat;

public sealed record GetConversationDetailsQuery(Guid ConversationId) : IRequest<Result<ConversationDetailsDto>>;

public sealed class GetConversationDetailsHandler(
    ApplicationDbContext dbContext,
    ICurrentUserProvider currentUserProvider) : IRequestHandler<GetConversationDetailsQuery, Result<ConversationDetailsDto>>
{
    public async Task<Result<ConversationDetailsDto>> Handle(GetConversationDetailsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserProvider.GetUserId();
        var conversation = await dbContext.Conversations
            .AsNoTracking()
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(
                x => x.Id == request.ConversationId && x.Participants.Any(p => p.UserId == currentUserId),
                cancellationToken);

        if (conversation is null)
        {
            return Result.NotFound<ConversationDetailsDto>("Диалог не найден.");
        }

        return Result.Ok(conversation.ToDetailsDto(currentUserId));
    }
}
