namespace TicketFlow.Infrastructure.Persistence.Models
{
    public sealed class TimeEntryRow
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public Guid AgentId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public DateOnly WorkDate { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
