using TicketFlow.Domain.Entities;
using System.Linq.Expressions;

namespace TicketFlow.Domain.Contracts
{
    public interface ITicketsRepository : IGenericRepository<Ticket>
    {
        Task<IEnumerable<Ticket>> GetFilteredTicketsAsync(
            Expression<Func<Ticket, bool>>? predicate,
            Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>>? orderBy,
            int pageNumber,
            int pageSize,
            params Expression<Func<Ticket, object>>[] includes);

        Task<int> CountFilteredTicketsAsync(Expression<Func<Ticket, bool>>? predicate);
    }
}
