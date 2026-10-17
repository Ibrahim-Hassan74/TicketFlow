using FluentValidation;

namespace TicketFlow.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("User id can't be blank.");

            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .WithMessage("Name can't be blank")
                .MaximumLength(100)
                .WithMessage("Name can't be longer than 100 characters");
        }
    }
}