using FluentValidation;

namespace TicketFlow.Application.Features.Authentication.Commands.Logout
{
    public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("User Id can't be blank.");
        }
    }
}
