namespace TicketFlow.Application.Common.Interfaces
{
    public interface ITicketAuthorizationService
    {
        Task<bool> CanAccessTicketAsync(Guid ticketId, Guid userId, string role, CancellationToken cancellationToken = default);
    }
}
