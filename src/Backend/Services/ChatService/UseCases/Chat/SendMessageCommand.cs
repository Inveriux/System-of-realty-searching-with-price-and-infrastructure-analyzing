using BasicBuildingBlocks.BusinessResults;
using BasicBuildingBlocks.Services.Abstractions;
using ChatService.Data;
using ChatService.DTOs.Chat;
using ChatService.Models;
using ChatService.Services.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.UseCases.Chat;

public sealed record SendMessageCommand(Guid ConversationId, SendMessageRequestDto Request) : IRequest<Result<MessageDto>>;

public sealed class SendMessageHandler(
    ApplicationDbContext dbContext,
    ICurrentUserProvider currentUserProvider,
    IChatRealtimeNotifier chatRealtimeNotifier) : IRequestHandler<SendMessageCommand, Result<MessageDto>>
{
    public async Task<Result<MessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserProvider.GetUserId();
        var currentUserRole = currentUserProvider.GetRole();
        var conversation = await dbContext.Conversations
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(
                x => x.Id == request.ConversationId && x.Participants.Any(p => p.UserId == currentUserId),
                cancellationToken);

        if (conversation is null)
        {
            return Result.NotFound<MessageDto>("Диалог не найден.");
        }

        var now = DateTime.UtcNow;
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderUserId = currentUserId,
            Text = request.Request.Text.Trim(),
            CreatedAtUtc = now,
            ClientMessageId = request.Request.ClientMessageId,
            IsRead = false
        };

        conversation.LastMessageAtUtc = now;
        conversation.LastMessagePreview = ChatMapping.BuildLastMessagePreview(message.Text);
        conversation.LastSenderUserId = currentUserId;
        conversation.UpdatedAtUtc = now;

        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = message.ToDto(currentUserId);
        await chatRealtimeNotifier.NotifyMessageReceivedAsync(conversation.Id, dto, cancellationToken);

        foreach (var participantId in conversation.Participants.Select(x => x.UserId))
        {
            var participantRole = participantId == currentUserId ? currentUserRole : "User";
            var item = await MarkConversationReadHandler.BuildConversationListItemAsync(
                dbContext,
                conversation.Id,
                participantId,
                participantRole,
                cancellationToken);
            if (item is not null)
            {
                await chatRealtimeNotifier.NotifyConversationUpdatedAsync(participantId, item, cancellationToken);
            }
        }

        return Result.Ok(dto);
    }
}
