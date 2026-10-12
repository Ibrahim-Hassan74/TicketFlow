using FluentValidation;

namespace TicketFlow.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email can't be blank.")
            .EmailAddress()
            .WithMessage("Invalid email address.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password can't be blank.")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long.");

            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .WithMessage("Display Name can't be blank.")
                .MaximumLength(100)
                .WithMessage("Display Name can't be longer than 100 characters.");

            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage("Role can't be blank.");
        }
    }
}
