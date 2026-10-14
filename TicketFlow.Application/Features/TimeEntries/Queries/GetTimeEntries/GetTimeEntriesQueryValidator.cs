using FluentValidation;

namespace TicketFlow.Application.Features.TimeEntries.Queries.GetTimeEntries
{
    public class GetTimeEntriesQueryValidator : AbstractValidator<GetTimeEntriesQuery>
    {
        public GetTimeEntriesQueryValidator()
        {
            RuleFor(x => x.TicketId)
                .NotEmpty().WithMessage("Ticket Id can't be blank.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User Id can't be blank.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role can't be blank.");
        }
    }
}