namespace TicketFlow.Infrastructure.Persistence.Models
{
    public sealed class UserRow
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public long TotalCount { get; set; }
    }
}
