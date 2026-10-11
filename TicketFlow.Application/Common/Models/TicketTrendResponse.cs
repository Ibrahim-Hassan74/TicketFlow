namespace TicketFlow.Application.Common.Models
{
    public class TicketTrendResponse
    {
        public DateOnly Date { get; set; }
        public int OpenCount { get; set; }
        public int ClosedCount { get; set; }
    }
}
