using MediatR;
using Asp.Versioning;
using TicketFlow.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.TimeEntries.DTOs;
using TicketFlow.Application.Features.TimeEntries.Commands.AddTimeEntry;
using TicketFlow.Application.Features.TimeEntries.Queries.GetTimeEntries;

namespace TicketFlow.API.Controllers
{
    /// <summary>
    /// Controller responsible for managing time entries on a specific support ticket.
    /// Provides endpoints for retrieving time logs and allowing assigned agents to log their work duration.
    /// </summary>
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/tickets/{ticketId:guid}/time-entries")]
    public class TicketTimeEntriesController : CustomControllerBase
    {
        private readonly ICurrentUserService _currentUserService;

        public TicketTimeEntriesController(IMediator mediator, ICurrentUserService currentUserService) : base(mediator)
        {
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Retrieves all time entries logged for a specific ticket.
        /// Access is restricted to Admins and the Agent explicitly assigned to the ticket.
        /// Customers cannot view time entries.
        /// </summary>
        /// <param name="ticketId">The unique identifier of the ticket.</param>
        /// <returns>A list of time entries along with the total duration logged.</returns>
        [HttpGet]
        [Authorize(Roles = $"{nameof(UserRole.SupportAgent)},{nameof(UserRole.Admin)}")]
        [ProducesResponseType(typeof(TicketTimeEntriesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTimeEntries(Guid ticketId)
        {
            var query = new GetTimeEntriesQuery()
            {
                TicketId = ticketId,
                UserId = _currentUserService.UserId,
                Role = _currentUserService.Role
            };

            var response = await _mediator.Send(query);

            return Ok(response);
        }

        /// <summary>
        /// Logs a new time entry for a specific ticket.
        /// Only the Support Agent currently assigned to the ticket can log time.
        /// </summary>
        /// <param name="ticketId">The unique identifier of the ticket.</param>
        /// <param name="request">The time entry details (WorkDate, DurationMinutes, Description).</param>
        /// <returns>A success response if the time entry is added.</returns>
        [HttpPost]
        [Authorize(Roles = nameof(UserRole.SupportAgent))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddTimeEntry(Guid ticketId, [FromBody] CreateTimeEntryRequest request)
        {
            var command = new AddTimeEntryCommand()
            {
                TicketId = ticketId,
                AgentId = _currentUserService.UserId,
                CreateTimeEntry = request
            };

            await _mediator.Send(command);

            return Ok();
        }
    }
}
