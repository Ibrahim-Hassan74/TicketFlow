namespace TicketFlow.Application.Features.Tickets.DTOs
{
    public class UpdateTicketRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
