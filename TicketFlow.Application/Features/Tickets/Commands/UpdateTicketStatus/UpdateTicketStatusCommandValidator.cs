using FluentValidation;

namespace TicketFlow.Application.Features.Tickets.Commands.UpdateTicketStatus
{
    public class UpdateTicketStatusCommandValidator : AbstractValidator<UpdateTicketStatusCommand>
    {
        public UpdateTicketStatusCommandValidator()
        {
            RuleFor(x => x.NewStatus)
                .IsInEnum()
                .WithMessage("Invalid ticket status.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User Id can't be blank.");

            RuleFor(x => x.TicketId)
                .NotEmpty()
                .WithMessage("Ticket Id can't be blank.");

            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage("Role can't be blank.");
        }
    }
}
