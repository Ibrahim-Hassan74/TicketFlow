using FluentValidation;

namespace TicketFlow.Application.Features.Tickets.Commands.UpdateTicket
{
    public class UpdateTicketCommandValidator : AbstractValidator<UpdateTicketCommand>
    {
        public UpdateTicketCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User Id can't be blank.");

            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Ticket Id can't be blank.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title can't be blank.")
                .MaximumLength(200)
                .WithMessage("Title can't be more than 200 char.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description can't be blank.")
                .MaximumLength(4000)
                .WithMessage("Description can't be more than 4000 char.");
        }
    }
}
