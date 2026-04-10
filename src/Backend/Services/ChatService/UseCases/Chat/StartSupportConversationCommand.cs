using BasicBuildingBlocks.BusinessResults;
using BasicBuildingBlocks.Services.Abstractions;
using ChatService.Clients.Users.Abstractions;
using ChatService.Data;
using ChatService.DTOs.Chat;
using ChatService.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.UseCases.Chat;

public sealed record StartSupportConversationCommand() : IRequest<Result<ConversationDetailsDto>>;

public sealed class StartSupportConversationHandler(
    ApplicationDbContext dbContext,
    ICurrentUserProvider currentUserProvider,
    IUserProfileClient userProfileClient) : IRequestHandler<StartSupportConversationCommand, Result<ConversationDetailsDto>>
{
    private static readonly Guid SupportListingId = Guid.Empty;

    public async Task<Result<ConversationDetailsDto>> Handle(StartSupportConversationCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserProvider.GetUserId();
        var currentUserRole = currentUserProvider.GetRole();
        if (string.Equals(currentUserRole, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Validation<ConversationDetailsDto>("Администратор не может открыть обращение в поддержку от своего имени.");
        }

        var supportProfile = await userProfileClient.GetSupportProfileAsync(cancellationToken);
        if (supportProfile is null)
        {
            return Result.NotFound<ConversationDetailsDto>("Оператор поддержки не найден.");
        }

        if (supportProfile.Id == currentUserId)
        {
            return Result.Validation<ConversationDetailsDto>("Нельзя создать диалог с самим собой.");
        }

        var userPairKey = ChatMapping.BuildUserPairKey(currentUserId, supportProfile.Id);
        var existingConversation = await dbContext.Conversations
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(
                x => x.ListingId == SupportListingId && x.UserPairKey == userPairKey,
                cancellationToken);

        if (existingConversation is not null)
        {
            return Result.Ok(existingConversation.ToDetailsDto(currentUserId, currentUserRole));
        }

        var now = DateTime.UtcNow;
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            ListingId = SupportListingId,
            ListingTitle = "Поддержка Domio",
            ListingPriceAmount = null,
            ListingPriceCurrency = null,
            ListingImageUrl = null,
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
                    UserId = supportProfile.Id,
                    DisplayName = supportProfile.FullName,
                    AvatarUrl = supportProfile.AvatarUrl,
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
                    x => x.ListingId == SupportListingId && x.UserPairKey == userPairKey,
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
