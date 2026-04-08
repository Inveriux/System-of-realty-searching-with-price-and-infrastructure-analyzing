using BasicBuildingBlocks.Services.Abstractions;
using ChatService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Hubs;

[Authorize]
public sealed class ChatHub(
    ApplicationDbContext dbContext,
    ICurrentUserProvider currentUserProvider) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, BuildUserGroupName(currentUserProvider.GetUserId()));
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildUserGroupName(currentUserProvider.GetUserId()));
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinConversation(Guid conversationId)
    {
        var userId = currentUserProvider.GetUserId();
        var isParticipant = await dbContext.ConversationParticipants
            .AnyAsync(
                x => x.ConversationId == conversationId && x.UserId == userId,
                Context.ConnectionAborted);

        if (!isParticipant)
        {
            throw new HubException("Доступ к диалогу запрещён.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, BuildConversationGroupName(conversationId), Context.ConnectionAborted);
    }

    public Task LeaveConversation(Guid conversationId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildConversationGroupName(conversationId), Context.ConnectionAborted);

    public static string BuildConversationGroupName(Guid conversationId) => $"conversation:{conversationId}";

    public static string BuildUserGroupName(Guid userId) => $"user:{userId}";
}
