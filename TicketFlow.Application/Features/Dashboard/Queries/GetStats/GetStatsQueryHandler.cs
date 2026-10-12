using MediatR;
using Microsoft.Extensions.Logging;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Models;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Dashboard.Queries.GetStats
{
    public sealed class GetStatsQueryHandler : IRequestHandler<GetStatsQuery, DashboardStatsResponse>
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly ILogger<GetStatsQueryHandler> _logger;

        public GetStatsQueryHandler(IDashboardRepository dashboardRepository, ILogger<GetStatsQueryHandler> logger)
        {
            _dashboardRepository = dashboardRepository;
            _logger = logger;
        }

        public async Task<DashboardStatsResponse> Handle(GetStatsQuery request, CancellationToken cancellationToken)
        {
            if (request.Role != nameof(UserRole.Admin))
            {
                _logger.LogWarning("Forbidden access attempt to Dashboard Stats by role {Role}.", role);
                throw new ForbiddenException("Only administrators can access dashboard metrics.");
            }

            var stats = await _dashboardRepository.GetDashboardStatsAsync();
            return  stats;
        }
    }
}
