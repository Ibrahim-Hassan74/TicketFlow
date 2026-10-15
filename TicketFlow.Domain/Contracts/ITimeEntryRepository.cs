using TicketFlow.Domain.Entities;

namespace TicketFlow.Domain.Contracts
{
    public interface ITimeEntryRepository : IGenericRepository<TimeEntry>
    {
        Task<IEnumerable<TimeEntry>> GetTimeEntriesByTicketIdAsync(Guid ticketId, CancellationToken ct = default);
    }
}
