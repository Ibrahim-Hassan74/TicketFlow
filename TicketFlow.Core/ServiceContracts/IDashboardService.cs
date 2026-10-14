using TicketFlow.Core.DTO;

namespace TicketFlow.Core.ServiceContracts
{
    public interface IDashboardService
    {
        Task<ApiResponse> GetStatsAsync(string role);
        Task<ApiResponse> GetAgentWorkloadAsync(string role);
        Task<ApiResponse> GetTicketTrendsAsync(int days, string role);
    }
}
