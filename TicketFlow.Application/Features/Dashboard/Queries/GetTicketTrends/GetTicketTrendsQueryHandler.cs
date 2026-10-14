using MediatR;
using Microsoft.Extensions.Logging;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Models;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Dashboard.Queries.GetTicketTrends
{
    public sealed class GetTicketTrendsQueryHandler : IRequestHandler<GetTicketTrendsQuery, IEnumerable<TicketTrendResponse>>
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly ILogger<GetTicketTrendsQueryHandler> _logger;

        public GetTicketTrendsQueryHandler(IDashboardRepository dashboardRepository, ILogger<GetTicketTrendsQueryHandler> logger)
        {
            _dashboardRepository = dashboardRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<TicketTrendResponse>> Handle(GetTicketTrendsQuery request, CancellationToken cancellationToken)
        {
            if (request.Role != nameof(UserRole.Admin))
            {
                _logger.LogWarning("Forbidden access attempt to Ticket Trends by role {Role}.", request.Role);
                throw new ForbiddenException("Only administrators can access dashboard metrics.");
            }

            var trends = await _dashboardRepository.GetTicketTrendsAsync(request.Days);
            return trends;
        }
    }
}
