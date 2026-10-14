using FluentValidation;

namespace TicketFlow.Application.Features.Tickets.Queries.GetTicketById
{
    public class GetTicketByIdQueryValidator : AbstractValidator<GetTicketByIdQuery>
    {
        public GetTicketByIdQueryValidator()
        {
            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage("Role can't be blank.");

            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Ticket Id can't be blank.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User Id can't be blank.");
        }
    }
}
