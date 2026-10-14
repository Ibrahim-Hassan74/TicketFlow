using FluentValidation;

namespace TicketFlow.Application.Features.TimeEntries.Commands.AddTimeEntry
{
    public class AddTimeEntryCommandValidator : AbstractValidator<AddTimeEntryCommand>
    {
        public AddTimeEntryCommandValidator()
        {

            RuleFor(x => x.WorkDate)
                .NotEmpty()
                .WithMessage("Work Date can't be blank.");

            RuleFor(x => x.DurationMinutes)
                .InclusiveBetween(1, 1440)
                .WithMessage("Duration must be between 1 and 1440 minutes (24 hours).");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description can't be blank.")
                .MaximumLength(500)
                .WithMessage("Description must not exceed 500 characters.");

            RuleFor(x => x.TicketId)
                .NotEmpty()
                .WithMessage("Ticket Id can't be blank.");

            RuleFor(x => x.AgentId)
                .NotEmpty()
                .WithMessage("Agent Id can't be blank.");
        }
    }
}
