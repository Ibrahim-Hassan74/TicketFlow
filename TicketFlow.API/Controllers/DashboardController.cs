using MediatR;
using Asp.Versioning;
using TicketFlow.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Dashboard.Queries.GetStats;
using TicketFlow.Application.Features.Dashboard.Queries.GetTicketTrends;
using TicketFlow.Application.Features.Dashboard.Queries.GetAgentWorkload;

namespace TicketFlow.API.Controllers
{
    /// <summary>
    /// Controller responsible for retrieving system-wide dashboard metrics.
    /// Provides endpoints for overall statistics, agent workload, and ticket trends.
    /// Restricted to administrators only.
    /// </summary>
    [ApiVersion(1.0)]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class DashboardController : CustomControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        public DashboardController(IMediator mediator, ICurrentUserService currentUserService) : base(mediator)
        {
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Retrieves system-wide ticket statistics.
        /// </summary>
        /// <returns>Counts for tickets in various states and average resolution time.</returns>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(DashboardStatsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetStats()
        {
            var role = _currentUserService.Role;

            var query = new GetStatsQuery() { Role = role };

            var response = await _mediator.Send(query);

            return Ok(response);
        }

        /// <summary>
        /// Retrieves the current workload for all agents.
        /// </summary>
        /// <returns>A list of agents with their open/in-progress ticket counts and total logged time.</returns>
        [HttpGet("agent-workload")]
        [ProducesResponseType(typeof(List<AgentWorkloadResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAgentWorkload()
        {
            var role = _currentUserService.Role;

            var query = new GetAgentWorkloadQuery() { Role = role };

            var response = await _mediator.Send(query);

            return Ok(response);
        }

        /// <summary>
        /// Retrieves ticket creation and closure trends over a specified number of days.
        /// </summary>
        /// <param name="days">The number of days to look back (default is 30).</param>
        /// <returns>A chronological list of daily ticket trends.</returns>
        [HttpGet("ticket-trends")]
        [ProducesResponseType(typeof(List<TicketTrendResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTicketTrends([FromQuery] int days = 30)
        {
            var role = _currentUserService.Role;

            var query = new GetTicketTrendsQuery() { Role = role, Days = days };

            var response = await _mediator.Send(query);

            return Ok(response);
        }
    }
}