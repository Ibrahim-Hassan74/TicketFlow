using MediatR;

namespace TicketFlow.Application.Features.Tickets.Commands.AssignAgent
{
    public sealed class AssignAgentCommand : IRequest<Unit>
    {
        public Guid TicketId { get; set; }
        public Guid AgentId { get; set; }
        public Guid UserId { get; set; }
    }
}
