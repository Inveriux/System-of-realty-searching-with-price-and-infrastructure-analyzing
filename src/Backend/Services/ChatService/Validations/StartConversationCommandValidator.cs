using ChatService.UseCases.Chat;
using FluentValidation;

namespace ChatService.Validations;

public sealed class StartConversationCommandValidator : AbstractValidator<StartConversationCommand>
{
    public StartConversationCommandValidator()
    {
        RuleFor(x => x.Request.ListingId)
            .NotEmpty()
            .WithMessage("listingId обязателен.");
    }
}
