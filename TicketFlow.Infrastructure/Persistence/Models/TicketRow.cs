namespace TicketFlow.Infrastructure.Persistence.Models
{
    public sealed class TicketRow
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Status { get; set; }
        public int Priority { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public Guid? AssignedAgentId { get; set; }
        public string? AssignedAgentName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? ResolvedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public long TotalCount { get; set; }
    }
}
