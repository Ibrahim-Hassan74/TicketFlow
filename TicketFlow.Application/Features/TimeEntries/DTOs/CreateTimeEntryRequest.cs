namespace TicketFlow.Application.Features.TimeEntries.DTOs
{
    public class CreateTimeEntryRequest
    {
        public DateOnly WorkDate { get; set; }

        public int DurationMinutes { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}