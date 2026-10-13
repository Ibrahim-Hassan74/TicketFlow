using FluentValidation;

namespace TicketFlow.Application.Features.Tickets.DTOs
{
    public class AssignTicketRequest
    {
        public Guid AgentId { get; set; }
    }
    public class AssignTicketRequestValidator : AbstractValidator<AssignTicketRequest>
    {
        public AssignTicketRequestValidator()
        {
            RuleFor(x => x.AgentId)
                .NotEmpty()
                .WithMessage("Agent Id can't be blank.");
        }
    }
}
