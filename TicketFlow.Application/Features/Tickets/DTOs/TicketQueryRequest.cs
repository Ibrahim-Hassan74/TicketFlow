using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Tickets.DTOs
{
    public class TicketQueryRequest
    {
        public TicketStatus? Status { get; set; }
        public TicketPriority? Priority { get; set; }
        public string? Search { get; set; }
        public SortByOptions? SortBy { get; set; }
        public SortOrderOptions? SortOrder { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
