using FluentValidation;

namespace TicketFlow.Application.Features.Dashboard.Queries.GetTicketTrends
{
    public class GetTicketTrendsQueryValidator : AbstractValidator<GetTicketTrendsQuery>
    {
        public GetTicketTrendsQueryValidator()
        {
            RuleFor(x => x.Days)
                .InclusiveBetween(1, 365)
                .WithMessage("Days parameter must be between 1 and 365.");

            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage("Role can't be blank.");
        }
    }
}
