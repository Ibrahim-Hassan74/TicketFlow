using TicketFlow.Application.Common.Models;

namespace TicketFlow.Application.Common.Interfaces
{
    public interface IDashboardRepository
    {
        Task<DashboardStatsResponse> GetDashboardStatsAsync(CancellationToken ct = default);
        Task<IEnumerable<AgentWorkloadResponse>> GetAgentWorkloadAsync(CancellationToken ct = default);
        Task<IEnumerable<TicketTrendResponse>> GetTicketTrendsAsync(int days, CancellationToken ct = default);
    }
}
