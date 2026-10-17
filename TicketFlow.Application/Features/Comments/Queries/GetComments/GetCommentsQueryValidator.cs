using FluentValidation;

namespace TicketFlow.Application.Features.Comments.Queries.GetComments
{
    public class GetCommentsQueryValidator : AbstractValidator<GetCommentsQuery>
    {
        public GetCommentsQueryValidator()
        {
            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage("Role can't be blank.");

            RuleFor(x => x.TicketId)
                .NotEmpty()
                .WithMessage("Ticket Id can't be blank.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User Id can't be blank.");
        }
    }
}
