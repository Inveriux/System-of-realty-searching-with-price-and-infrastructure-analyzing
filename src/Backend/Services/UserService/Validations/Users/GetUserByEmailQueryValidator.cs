using UserService.UseCases.Users;
using FluentValidation;

namespace UserService.Validations.Users;

public class GetUserByEmailQueryValidator : AbstractValidator<GetUserByEmailQuery>
{
    public GetUserByEmailQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Электронная почта обязательна.")
            .EmailAddress()
            .WithMessage("Некорректный формат email.");
    }
}
