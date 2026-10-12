using FluentValidation;

namespace TicketFlow.Application.Features.TimeEntries.DTOs
{
    public class CreateTimeEntryRequest
    {
        public DateOnly WorkDate { get; set; }

        public int DurationMinutes { get; set; }

        public string Description { get; set; } = string.Empty;
    }

    public class CreateTimeEntryRequestValidator : AbstractValidator<CreateTimeEntryRequest>
    {
        public CreateTimeEntryRequestValidator()
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
        }
    }
}