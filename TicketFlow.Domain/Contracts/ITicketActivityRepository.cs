using TicketFlow.Domain.Entities;

namespace TicketFlow.Domain.Contracts
{
    public interface ITicketActivityRepository : IGenericRepository<TicketActivity>
    {
        Task<IEnumerable<TicketActivity>> GetActivitiesByTicketIdAsync(Guid ticketId, CancellationToken ct = default);
    }
}
