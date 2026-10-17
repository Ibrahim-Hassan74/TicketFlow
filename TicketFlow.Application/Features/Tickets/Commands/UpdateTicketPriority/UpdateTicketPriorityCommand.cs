using MediatR;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Tickets.Commands.UpdateTicketPriority
{
    public sealed class UpdateTicketPriorityCommand : IRequest<Unit>
    {
        public Guid TicketId { get; set; }
        public TicketPriority NewPriority { get; set; }
        public Guid UserId { get; set; }
    }
}
