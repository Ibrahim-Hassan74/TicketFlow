using TicketFlow.Domain.Entities;
using System.Linq.Expressions;

namespace TicketFlow.Domain.Contracts
{
    public interface ITicketsRepository : IGenericRepository<Ticket>
    {
        Task<(IEnumerable<Ticket> Items, int TotalCount)> GetTicketsPagedAsync(
            string? role,
            Guid userId,
            TicketFlow.Domain.Enums.TicketStatus? status,
            TicketFlow.Domain.Enums.TicketPriority? priority,
            string? search,
            int page,
            int pageSize,
            string? sortBy,
            string? sortOrder,
            CancellationToken ct = default);

        Task<IEnumerable<Ticket>> GetFilteredTicketsAsync(
            Expression<Func<Ticket, bool>>? predicate,
            Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>>? orderBy,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default,
            params Expression<Func<Ticket, object>>[] includes);

        Task<int> CountFilteredTicketsAsync(Expression<Func<Ticket, bool>>? predicate, CancellationToken ct = default);
    }
}
