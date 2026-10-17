using FluentValidation;

namespace TicketFlow.Application.Features.Authentication.Commands.RefreshToken
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.Token).NotEmpty().WithMessage("Token is required.");
            RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh Token is required.");
        }

    }
}
