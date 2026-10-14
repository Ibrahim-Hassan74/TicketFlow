using FluentValidation;

namespace TicketFlow.Application.Features.Tickets.Commands.AssignAgent
{
    public class AssignAgentCommandValidator : AbstractValidator<AssignAgentCommand>
    {
        public AssignAgentCommandValidator()
        {
            RuleFor(x => x.TicketId)
                .NotEmpty()
                .WithMessage("Ticket Id can't be blank.");

            RuleFor(x => x.AgentId)
                .NotEmpty()
                .WithMessage("Agent Id can't be blank.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User Id can't be blank.");

        }
    }
}
