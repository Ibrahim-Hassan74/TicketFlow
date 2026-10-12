using MediatR;
using Microsoft.Extensions.Logging;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Common.Helpers;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Models;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Dashboard.Queries.GetAgentWorkload
{
    public sealed class GetAgentWorkloadQueryHandler : IRequestHandler<GetAgentWorkloadQuery, IEnumerable<AgentWorkloadResponse>>
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly ILogger<GetAgentWorkloadQueryHandler> _logger;

        public GetAgentWorkloadQueryHandler(IDashboardRepository dashboardRepository, ILogger<GetAgentWorkloadQueryHandler> logger)
        {
            _dashboardRepository = dashboardRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<AgentWorkloadResponse>> Handle(GetAgentWorkloadQuery request, CancellationToken cancellationToken)
        {
            if (request.Role != nameof(UserRole.Admin))
            {
                _logger.LogWarning("Forbidden access attempt to Agent Workload by role {Role}.", request.Role);
                
                throw new ForbiddenException("Only administrators can access dashboard metrics.");
            }

            var workload = await _dashboardRepository.GetAgentWorkloadAsync(cancellationToken);
            return workload;
        }
    }
}
