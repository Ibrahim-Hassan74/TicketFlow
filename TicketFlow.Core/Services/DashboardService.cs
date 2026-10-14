using TicketFlow.Core.DTO;
using TicketFlow.Core.Enums;
using TicketFlow.Core.Helper;
using TicketFlow.Core.RepositoryContracts;
using TicketFlow.Core.ServiceContracts;
using Microsoft.Extensions.Logging;

namespace TicketFlow.Core.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(IUnitOfWork unitOfWork, ILogger<DashboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse> GetAgentWorkloadAsync(string role)
        {
            if (role != nameof(UserRole.Admin))
            {
                _logger.LogWarning("Forbidden access attempt to Agent Workload by role {Role}.", role);
                return ApiResponseFactory.Forbidden("Only administrators can access dashboard metrics.");
            }

            var workload = await _unitOfWork.TicketsRepository.GetAgentWorkloadAsync();
            return ApiResponseFactory.Success("Agent workload retrieved successfully.", workload);
        }

        public async Task<ApiResponse> GetStatsAsync(string role)
        {
            if (role != nameof(UserRole.Admin))
            {
                _logger.LogWarning("Forbidden access attempt to Dashboard Stats by role {Role}.", role);
                return ApiResponseFactory.Forbidden("Only administrators can access dashboard metrics.");
            }

            var stats = await _unitOfWork.TicketsRepository.GetDashboardStatsAsync();
            return ApiResponseFactory.Success("Dashboard stats retrieved successfully.", stats);
        }

        public async Task<ApiResponse> GetTicketTrendsAsync(int days, string role)
        {
            if (role != nameof(UserRole.Admin))
            {
                _logger.LogWarning("Forbidden access attempt to Ticket Trends by role {Role}.", role);
                return ApiResponseFactory.Forbidden("Only administrators can access dashboard metrics.");
            }

            if (days <= 0 || days > 365)
                return ApiResponseFactory.BadRequest("Days parameter must be between 1 and 365.");

            var trends = await _unitOfWork.TicketsRepository.GetTicketTrendsAsync(days);
            return ApiResponseFactory.Success("Ticket trends retrieved successfully.", trends);
        }
    }
}
