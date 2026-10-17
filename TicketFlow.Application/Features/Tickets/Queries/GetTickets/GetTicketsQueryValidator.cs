using FluentValidation;

namespace TicketFlow.Application.Features.Tickets.Queries.GetTickets
{
    public class GetTicketsQueryValidator : AbstractValidator<GetTicketsQuery>
    {
        public GetTicketsQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page number must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid ticket status.")
                .When(x => x.Status.HasValue);

            RuleFor(x => x.Priority)
                .IsInEnum()
                .WithMessage("Invalid ticket priority.")
                .When(x => x.Priority.HasValue);

            RuleFor(x => x.SortBy)
                .IsInEnum()
                .WithMessage("Invalid sort field.")
                .When(x => x.SortBy.HasValue);

            RuleFor(x => x.SortOrder)
                .IsInEnum()
                .WithMessage("Invalid sort order.")
                .When(x => x.SortOrder.HasValue);
        }
    }
}
