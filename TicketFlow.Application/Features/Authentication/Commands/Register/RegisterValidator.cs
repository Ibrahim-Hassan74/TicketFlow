using FluentValidation;

namespace TicketFlow.Application.Features.Authentication.Commands.Register
{
    public class RegisterValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("{PropertyName} can't be blank")
            .WithName("User Name");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("{PropertyName} can't be blank")
                .EmailAddress()
                .WithMessage("{PropertyName} should be in a proper email address format");

            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("{PropertyName} can't be blank")
                .Matches("^[0-9]*$")
                .WithMessage("{PropertyName} should contains numbers only");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("{PropertyName} can't be blank")
                .MinimumLength(8)
                .WithMessage("{PropertyName} should be at least 8 characters");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("{PropertyName} can't be blank")
                .Equal(x => x.Password)
                .WithMessage("Passwords do not match");
        }
    }
}
