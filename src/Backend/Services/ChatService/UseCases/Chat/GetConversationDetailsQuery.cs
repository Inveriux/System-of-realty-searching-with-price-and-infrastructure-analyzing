using BasicBuildingBlocks.BusinessResults;
using BasicBuildingBlocks.Services.Abstractions;
using ChatService.Clients.Users.Abstractions;
using ChatService.Data;
using ChatService.DTOs.Chat;
using ChatService.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.UseCases.Chat;

public sealed record GetConversationDetailsQuery(Guid ConversationId) : IRequest<Result<ConversationDetailsDto>>;

public sealed class GetConversationDetailsHandler(
    ApplicationDbContext dbContext,
    ICurrentUserProvider currentUserProvider,
    IUserProfileClient userProfileClient) : IRequestHandler<GetConversationDetailsQuery, Result<ConversationDetailsDto>>
{
    public async Task<Result<ConversationDetailsDto>> Handle(GetConversationDetailsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserProvider.GetUserId();
        var currentUserRole = currentUserProvider.GetRole();
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

        var details = conversation.ToDetailsDto(currentUserId, currentUserRole);
        if (conversation.GetConversationType() != ConversationTypes.Support
            || !string.Equals(currentUserRole, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Ok(details);
        }

        var profile = await userProfileClient.GetPublicProfileAsync(details.OtherUserId, cancellationToken);
        if (profile is null)
        {
            return Result.Ok(details);
        }

        return Result.Ok(new ConversationDetailsDto
        {
            ConversationId = details.ConversationId,
            ConversationType = details.ConversationType,
            ListingId = details.ListingId,
            ListingTitle = details.ListingTitle,
            ListingPrice = details.ListingPrice,
            ListingPriceCurrency = details.ListingPriceCurrency,
            ListingImageUrl = details.ListingImageUrl,
            OtherUserId = details.OtherUserId,
            OtherUserName = details.OtherUserName,
            OtherUserAvatarUrl = details.OtherUserAvatarUrl,
            OtherUserEmail = profile.Email,
            OtherUserPhone = profile.Phone
        });
    }
}
