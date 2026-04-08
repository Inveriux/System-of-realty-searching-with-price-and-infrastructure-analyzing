using BasicBuildingBlocks.BusinessResults.Extensoins;
using BasicBuildingBlocks.Endpoints.Abstractions;
using ChatService.DTOs.Chat;
using ChatService.UseCases.Chat;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Endpoints;

public sealed class ChatEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var group = application.MapGroup("/api/chat")
            .WithTags("Chat")
            .RequireAuthorization();

        group.MapPost("/conversations/start", StartConversationAsync);
        group.MapGet("/conversations", GetMyConversationsAsync);
        group.MapGet("/conversations/{conversationId:guid}", GetConversationDetailsAsync);
        group.MapGet("/conversations/{conversationId:guid}/messages", GetConversationMessagesAsync);
        group.MapPost("/conversations/{conversationId:guid}/messages", SendMessageAsync);
        group.MapPost("/conversations/{conversationId:guid}/read", MarkConversationAsReadAsync);
        group.MapGet("/unread-count", GetUnreadCountAsync);

        return group;
    }

    public async Task<IResult> StartConversationAsync(
        ISender sender,
        StartConversationRequestDto request,
        CancellationToken cancellationToken)
        => (await sender.Send(new StartConversationCommand(request), cancellationToken)).ToHttpResult();

    public async Task<IResult> GetMyConversationsAsync(
        ISender sender,
        CancellationToken cancellationToken)
        => (await sender.Send(new GetMyConversationsQuery(), cancellationToken)).ToHttpResult();

    public async Task<IResult> GetConversationDetailsAsync(
        ISender sender,
        [FromRoute] Guid conversationId,
        CancellationToken cancellationToken)
        => (await sender.Send(new GetConversationDetailsQuery(conversationId), cancellationToken)).ToHttpResult();

    public async Task<IResult> GetConversationMessagesAsync(
        ISender sender,
        [FromRoute] Guid conversationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken cancellationToken = default)
        => (await sender.Send(new GetConversationMessagesQuery(conversationId, page, pageSize), cancellationToken)).ToHttpResult();

    public async Task<IResult> SendMessageAsync(
        ISender sender,
        [FromRoute] Guid conversationId,
        SendMessageRequestDto request,
        CancellationToken cancellationToken)
        => (await sender.Send(new SendMessageCommand(conversationId, request), cancellationToken)).ToHttpResult();

    public async Task<IResult> MarkConversationAsReadAsync(
        ISender sender,
        [FromRoute] Guid conversationId,
        CancellationToken cancellationToken)
        => (await sender.Send(new MarkConversationReadCommand(conversationId), cancellationToken)).ToHttpResult();

    public async Task<IResult> GetUnreadCountAsync(
        ISender sender,
        CancellationToken cancellationToken)
        => (await sender.Send(new GetUnreadCountQuery(), cancellationToken)).ToHttpResult();
}
