using ChatService.UseCases.Chat;
using FluentValidation;

namespace ChatService.Validations;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.Request.Text)
            .NotEmpty()
            .MaximumLength(4000);
        RuleFor(x => x.Request.ClientMessageId)
            .MaximumLength(100);
    }
}
