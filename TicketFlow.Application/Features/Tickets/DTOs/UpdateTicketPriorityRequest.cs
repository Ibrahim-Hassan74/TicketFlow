using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Tickets.DTOs
{
    public class UpdateTicketPriorityRequest
    {
        public TicketPriority Priority { get; set; }
    }
}
