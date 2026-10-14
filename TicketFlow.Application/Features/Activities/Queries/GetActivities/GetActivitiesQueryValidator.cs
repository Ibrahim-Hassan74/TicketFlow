using FluentValidation;

namespace TicketFlow.Application.Features.Activities.Queries.GetActivities
{
    public class GetActivitiesQueryValidator : AbstractValidator<GetActivitiesQuery>
    {
        public GetActivitiesQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID can't be blank.");

            RuleFor(x => x.TicketId)
                .NotEmpty()
                .WithMessage("Ticket ID can't be blank.");
            
            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage("Role can't be blank.");
        }
    }
}
