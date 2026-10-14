using MediatR;
using Asp.Versioning;
using TicketFlow.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Tickets.DTOs;
using TicketFlow.Application.Features.Tickets.Queries.GetTickets;
using TicketFlow.Application.Features.Tickets.Commands.AssignAgent;
using TicketFlow.Application.Features.Tickets.Queries.GetTicketById;
using TicketFlow.Application.Features.Tickets.Commands.UpdateTicket;
using TicketFlow.Application.Features.Tickets.Commands.CreateTicket;
using TicketFlow.Application.Features.Tickets.Commands.UpdateTicketStatus;
using TicketFlow.Application.Features.Tickets.Commands.UpdateTicketPriority;

namespace TicketFlow.API.Controllers
{
    /// <summary>
    /// Controller responsible for managing support tickets.
    /// Provides endpoints for creating, retrieving, updating, and assigning tickets.
    /// Uses role-based logic to isolate data appropriately (Admin, SupportAgent, Customer).
    /// </summary>
    [ApiVersion(1.0)]
    public class TicketsController : CustomControllerBase
    {
        private readonly ICurrentUserService _currentUserService;

        public TicketsController(IMediator mediator, ICurrentUserService currentUserService) : base(mediator)
        {
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Retrieves a paginated list of tickets.
        /// Results are filtered based on the user's role:
        /// Admins see all tickets, Agents see only assigned tickets, Customers see only their own tickets.
        /// </summary>
        /// <param name="request">Pagination, filtering, and sorting parameters.</param>
        /// <returns>A paginated list of tickets.</returns>
        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<TicketResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTickets([FromQuery] TicketQueryRequest request)
        {
            var query = new GetTicketsQuery
            {
                Status = request.Status,
                Priority = request.Priority,
                Search = request.Search,
                SortBy = request.SortBy,
                SortOrder = request.SortOrder,
                Page = request.Page,
                PageSize = request.PageSize,
                UserId = _currentUserService.UserId,
                Role = _currentUserService.Role
            };

            var response = await _mediator.Send(query);
            return Ok(response);
        }

        /// <summary>
        /// Retrieves a specific ticket by its ID.
        /// Access is restricted based on the user's role and ownership/assignment of the ticket.
        /// </summary>
        /// <param name="id">The unique identifier of the ticket.</param>
        /// <returns>The ticket details.</returns>
        [Authorize]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTicketById(Guid id)
        {
            var query = new GetTicketByIdQuery(id, _currentUserService.UserId, _currentUserService.Role);

            var response = await _mediator.Send(query);

            return Ok(response);
        }

        /// <summary>
        /// Creates a new support ticket.
        /// Only users with the Customer role can create tickets.
        /// </summary>
        /// <param name="request">The ticket details (Title, Description, Priority).</param>
        /// <returns>The newly created ticket.</returns>
        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Customer))]
        [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
        {
            var command = new CreateTicketCommand()
            {
                CustomerId = _currentUserService.UserId,
                CreateTicket = request
            };

            var response = await _mediator.Send(command);

            return Ok(response);
        }

        /// <summary>
        /// Updates the core details (Title, Description) of a ticket.
        /// Only users with the Admin role can perform this general update.
        /// </summary>
        /// <param name="id">The unique identifier of the ticket.</param>
        /// <param name="request">The updated ticket details.</param>
        /// <returns>Success response.</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTicket(Guid id, [FromBody] UpdateTicketRequest request)
        {
            var command = new UpdateTicketCommand()
            {
                UpdateTicket = request,
                Id = id,
                UserId = _currentUserService.UserId
            };

            var response = await _mediator.Send(command);

            return Ok(response);
        }

        /// <summary>
        /// Updates the status of a ticket.
        /// Role rules apply:
        /// - Admin: Can set any status.
        /// - SupportAgent: Can resolve assigned tickets.
        /// - Customer: Can close or reopen their own tickets.
        /// </summary>
        /// <param name="id">The unique identifier of the ticket.</param>
        /// <param name="request">The new status.</param>
        /// <returns>Success response.</returns>
        [Authorize]
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTicketStatus(Guid id, [FromBody] UpdateTicketStatusRequest request)
        {
            var command = new UpdateTicketStatusCommand()
            {
                TicketId = id,
                NewStatus = request.Status,
                UserId = _currentUserService.UserId,
                Role = _currentUserService.Role
            };

            await _mediator.Send(command);

            return Ok();
        }

        /// <summary>
        /// Updates the priority of a ticket.
        /// Only users with the Admin role can change ticket priority.
        /// </summary>
        /// <param name="id">The unique identifier of the ticket.</param>
        /// <param name="request">The new priority.</param>
        /// <returns>Success response.</returns>
        [HttpPatch("{id:guid}/priority")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTicketPriority(Guid id, [FromBody] UpdateTicketPriorityRequest request)
        {
            var command = new UpdateTicketPriorityCommand()
            {
                TicketId = id,
                NewPriority = request.Priority,
                UserId = _currentUserService.UserId
            };

            await _mediator.Send(command);

            return Ok();
        }

        /// <summary>
        /// Assigns a Support Agent to a ticket.
        /// Only users with the Admin role can assign agents.
        /// </summary>
        /// <param name="id">The unique identifier of the ticket.</param>
        /// <param name="request">The ID of the Support Agent.</param>
        /// <returns>Success response.</returns>
        [HttpPatch("{id:guid}/assign")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignAgent(Guid id, [FromBody] AssignTicketRequest request)
        {
            var command = new AssignAgentCommand()
            {
                TicketId = id,
                AgentId = request.AgentId,
                UserId = _currentUserService.UserId
            };

            await _mediator.Send(command);

            return Ok();
        }
    }
}
