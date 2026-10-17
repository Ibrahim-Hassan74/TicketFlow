using TicketFlow.Domain.Entities;

namespace TicketFlow.Domain.Contracts
{
    public interface ITicketCommentRepository : IGenericRepository<TicketComment>
    {
        Task<IEnumerable<TicketComment>> GetCommentsByTicketIdAsync(Guid ticketId, CancellationToken ct = default);
    }
}
