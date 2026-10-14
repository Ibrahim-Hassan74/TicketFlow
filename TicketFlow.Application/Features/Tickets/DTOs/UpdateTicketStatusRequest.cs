using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Tickets.DTOs
{
    public class UpdateTicketStatusRequest
    {
        public TicketStatus Status { get; set; }
    }
}
