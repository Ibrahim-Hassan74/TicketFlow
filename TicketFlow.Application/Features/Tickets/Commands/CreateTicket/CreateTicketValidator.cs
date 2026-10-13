using FluentValidation;

namespace TicketFlow.Application.Features.Tickets.Commands.CreateTicket
{
    public class CreateTicketValidator : AbstractValidator<CreateTicketCommand>
    {
        public CreateTicketValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty()
                .WithMessage("Customer Id can't be blank.");

            RuleFor(x => x.CreateTicket)
                .NotNull()
                .WithMessage("Create Ticket can't be null");
        }
    }
}
