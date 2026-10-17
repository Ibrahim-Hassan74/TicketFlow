namespace TicketFlow.Application.Features.Users.DTOs
{
    public class UpdateUserRequest
    {
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
