using Dapper;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Models;

namespace TicketFlow.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// High-performance Dapper repository calling PostgreSQL stored functions for dashboard metrics.
    /// </summary>
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public DashboardRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<DashboardStatsResponse> GetDashboardStatsAsync(CancellationToken ct = default)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM sp_get_dashboard_stats();";

            var stats = await connection.QueryFirstOrDefaultAsync<DashboardStatsResponse>(
                new CommandDefinition(sql, cancellationToken: ct));

            return stats ?? new DashboardStatsResponse();
        }

        public async Task<IEnumerable<AgentWorkloadResponse>> GetAgentWorkloadAsync(CancellationToken ct = default)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM sp_get_agent_workload();";

            var workload = await connection.QueryAsync<AgentWorkloadResponse>(
                new CommandDefinition(sql, cancellationToken: ct));

            return workload;
        }

        public async Task<IEnumerable<TicketTrendResponse>> GetTicketTrendsAsync(int days, CancellationToken ct = default)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM sp_get_ticket_trends(@Days);";

            var trends = await connection.QueryAsync<TicketTrendResponse>(
                new CommandDefinition(sql, new { Days = days }, cancellationToken: ct));

            return trends;
        }
    }
}
