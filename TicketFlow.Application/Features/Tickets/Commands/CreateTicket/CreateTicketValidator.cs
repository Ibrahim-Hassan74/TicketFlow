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

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title can't be blank.")
                .MaximumLength(200)
                .WithMessage("Title can't be longer than 200 characters.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description can't be blank.")
                .MaximumLength(4000)
                .WithMessage("Description can't be longer than 4000 characters.");

            RuleFor(x => x.Priority)
                .IsInEnum()
                .WithMessage("Invalid ticket priority.");
        }
    }
}
