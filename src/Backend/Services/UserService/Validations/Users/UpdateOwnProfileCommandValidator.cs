using FluentValidation;
using UserService.UseCases.Users;

namespace UserService.Validations.Users;

public sealed class UpdateOwnProfileCommandValidator : AbstractValidator<UpdateOwnProfileCommand>
{
    public UpdateOwnProfileCommandValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Dto.PhoneNumber)
            .MaximumLength(32)
            .Matches(@"^[\d\s+\-()]*$")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.PhoneNumber))
            .WithMessage("Номер телефона содержит недопустимые символы.");
    }
}
