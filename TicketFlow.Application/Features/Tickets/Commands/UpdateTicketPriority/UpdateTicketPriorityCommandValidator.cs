using FluentValidation;

namespace TicketFlow.Application.Features.Tickets.Commands.UpdateTicketPriority
{
    public class UpdateTicketPriorityCommandValidator : AbstractValidator<UpdateTicketPriorityCommand>
    {
        public UpdateTicketPriorityCommandValidator()
        {
            RuleFor(x => x.NewPriority)
                .IsInEnum()
                .WithMessage("Invalid ticket priority.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User Id can't be blank.");

            RuleFor(x => x.TicketId)
                .NotEmpty()
                .WithMessage("Ticket Id can't be blank.");
        }
    }
}
