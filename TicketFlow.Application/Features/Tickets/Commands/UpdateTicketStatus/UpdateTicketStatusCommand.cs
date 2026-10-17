using MediatR;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Tickets.Commands.UpdateTicketStatus
{
    public sealed class UpdateTicketStatusCommand : IRequest<Unit>
    {
        public Guid TicketId { get; set; }
        public TicketStatus NewStatus { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
