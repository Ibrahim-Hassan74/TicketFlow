using TicketFlow.Core.Domain.Entities;
using TicketFlow.Core.DTO;
using System.Linq.Expressions;

namespace TicketFlow.Core.RepositoryContracts
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

        Task<DashboardStatsResponse> GetDashboardStatsAsync();
        Task<IEnumerable<AgentWorkloadResponse>> GetAgentWorkloadAsync();
        Task<IEnumerable<TicketTrendResponse>> GetTicketTrendsAsync(int days);
    }
}
