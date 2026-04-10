using BasicBuildingBlocks.BusinessResults;
using BasicBuildingBlocks.Services.Abstractions;
using ChatService.Clients.Listings.Abstractions;
using ChatService.Clients.Users.Abstractions;
using ChatService.Data;
using ChatService.DTOs.Chat;
using ChatService.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.UseCases.Chat;

public sealed record StartConversationCommand(StartConversationRequestDto Request) : IRequest<Result<ConversationDetailsDto>>;

public sealed class StartConversationHandler(
    ApplicationDbContext dbContext,
    ICurrentUserProvider currentUserProvider,
    IListingCatalogClient listingCatalogClient,
    IUserProfileClient userProfileClient) : IRequestHandler<StartConversationCommand, Result<ConversationDetailsDto>>
{
    public async Task<Result<ConversationDetailsDto>> Handle(StartConversationCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserProvider.GetUserId();
        var currentUserRole = currentUserProvider.GetRole();
        var listing = await listingCatalogClient.GetListingAsync(request.Request.ListingId, cancellationToken);
        if (listing is null)
        {
            return Result.NotFound<ConversationDetailsDto>("Объявление не найдено или уже удалено.");
        }

        var otherUserId = request.Request.OtherUserId ?? listing.AgentId;
        if (otherUserId == currentUserId)
        {
            return Result.Validation<ConversationDetailsDto>("Нельзя создать диалог с самим собой.");
        }

        if (otherUserId != listing.AgentId)
        {
            return Result.Validation<ConversationDetailsDto>("Можно открыть диалог только с владельцем объявления.");
        }

        var userPairKey = ChatMapping.BuildUserPairKey(currentUserId, otherUserId);
        var existingConversation = await dbContext.Conversations
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(
                x => x.ListingId == listing.Id && x.UserPairKey == userPairKey,
                cancellationToken);

        if (existingConversation is not null)
        {
            return Result.Ok(existingConversation.ToDetailsDto(currentUserId, currentUserRole));
        }

        var otherUserProfile = await userProfileClient.GetPublicProfileAsync(otherUserId, cancellationToken);
        if (otherUserProfile is null)
        {
            return Result.NotFound<ConversationDetailsDto>("Продавец не найден.");
        }

        var now = DateTime.UtcNow;
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            ListingId = listing.Id,
            ListingTitle = listing.Title,
            ListingPriceAmount = listing.PriceAmount,
            ListingPriceCurrency = listing.PriceCurrency,
            ListingImageUrl = listing.MainImageUrl,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedByUserId = currentUserId,
            UserPairKey = userPairKey,
            Participants =
            [
                new ConversationParticipant
                {
                    ConversationId = Guid.Empty,
                    UserId = currentUserId,
                    DisplayName = currentUserProvider.GetUserName(),
                    AvatarUrl = currentUserProvider.GetImageUrl(),
                    JoinedAtUtc = now
                },
                new ConversationParticipant
                {
                    ConversationId = Guid.Empty,
                    UserId = otherUserProfile.Id,
                    DisplayName = otherUserProfile.FullName,
                    AvatarUrl = otherUserProfile.AvatarUrl,
                    JoinedAtUtc = now
                }
            ]
        };

        foreach (var participant in conversation.Participants)
        {
            participant.ConversationId = conversation.Id;
        }

        dbContext.Conversations.Add(conversation);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var createdByOtherRequest = await dbContext.Conversations
                .Include(x => x.Participants)
                .FirstOrDefaultAsync(
                    x => x.ListingId == listing.Id && x.UserPairKey == userPairKey,
                    cancellationToken);

            if (createdByOtherRequest is not null)
            {
                return Result.Ok(createdByOtherRequest.ToDetailsDto(currentUserId, currentUserRole));
            }

            throw;
        }

        return Result.Ok(conversation.ToDetailsDto(currentUserId, currentUserRole));
    }
}
