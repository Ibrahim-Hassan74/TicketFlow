using MediatR;
using Asp.Versioning;
using TicketFlow.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.API.Attributes;
using Microsoft.AspNetCore.Authorization;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Dashboard.Queries.GetStats;
using TicketFlow.Application.Features.Dashboard.Queries.GetTicketTrends;
using TicketFlow.Application.Features.Dashboard.Queries.GetAgentWorkload;

namespace TicketFlow.API.Controllers
{
    /// <include file='../Docs/DashboardDocs.xml' path='docs/members[@name="DashboardController"]/Class/*' />
    [ApiVersion(1.0)]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesProblem(StatusCodes.Status401Unauthorized)]
    [ProducesProblem(StatusCodes.Status403Forbidden)]
    public class DashboardController : CustomControllerBase
    {
        private readonly ICurrentUserService _currentUserService;

        /// <include file='../Docs/DashboardDocs.xml' path='docs/members[@name="DashboardController"]/Constructor/*' />
        public DashboardController(IMediator mediator, ICurrentUserService currentUserService) : base(mediator)
        {
            _currentUserService = currentUserService;
        }

        /// <include file='../Docs/DashboardDocs.xml' path='docs/members[@name="DashboardController"]/GetStats/*' />
        [HttpGet("stats")]
        [ProducesResponseType(typeof(DashboardStatsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStats()
        {
            var role = _currentUserService.Role;

            var query = new GetStatsQuery() { Role = role };

            var response = await _mediator.Send(query);

            return Ok(response);
        }

        /// <include file='../Docs/DashboardDocs.xml' path='docs/members[@name="DashboardController"]/GetAgentWorkload/*' />
        [HttpGet("agent-workload")]
        [ProducesResponseType(typeof(List<AgentWorkloadResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAgentWorkload()
        {
            var role = _currentUserService.Role;

            var query = new GetAgentWorkloadQuery() { Role = role };

            var response = await _mediator.Send(query);

            return Ok(response);
        }

        /// <include file='../Docs/DashboardDocs.xml' path='docs/members[@name="DashboardController"]/GetTicketTrends/*' />
        [HttpGet("ticket-trends")]
        [ProducesResponseType(typeof(List<TicketTrendResponse>), StatusCodes.Status200OK)]
        [ProducesProblem(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTicketTrends([FromQuery] int days = 30)
        {
            var role = _currentUserService.Role;

            var query = new GetTicketTrendsQuery() { Role = role, Days = days };

            var response = await _mediator.Send(query);

            return Ok(response);
        }
    }
}