using MediatR;
using Asp.Versioning;
using TicketFlow.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.API.Attributes;
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
    /// <include file='../Docs/TicketsDocs.xml' path='docs/members[@name="TicketsController"]/Class/*' />
    [ApiVersion(1.0)]
    [ProducesProblem(StatusCodes.Status400BadRequest)]
    [ProducesProblem(StatusCodes.Status401Unauthorized)]
    public class TicketsController : CustomControllerBase
    {
        private readonly ICurrentUserService _currentUserService;

        /// <include file='../Docs/TicketsDocs.xml' path='docs/members[@name="TicketsController"]/Constructor/*' />
        public TicketsController(IMediator mediator, ICurrentUserService currentUserService) : base(mediator)
        {
            _currentUserService = currentUserService;
        }

        /// <include file='../Docs/TicketsDocs.xml' path='docs/members[@name="TicketsController"]/GetTickets/*' />
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(PaginatedResponse<TicketResponse>), StatusCodes.Status200OK)]
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

        /// <include file='../Docs/TicketsDocs.xml' path='docs/members[@name="TicketsController"]/GetTicketById/*' />
        [Authorize]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
        [ProducesProblem(StatusCodes.Status401Unauthorized)]
        [ProducesProblem(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTicketById(Guid id)
        {
            var query = new GetTicketByIdQuery(id, _currentUserService.UserId, _currentUserService.Role);

            var response = await _mediator.Send(query);

            return Ok(response);
        }

        /// <include file='../Docs/TicketsDocs.xml' path='docs/members[@name="TicketsController"]/CreateTicket/*' />
        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Customer))]
        [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status201Created)]
        [ProducesProblem(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
        {
            var command = new CreateTicketCommand()
            {
                CustomerId = _currentUserService.UserId,
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority
            };

            var response = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetTicketById), new { id = response.Id }, response);
        }


        /// <include file='../Docs/TicketsDocs.xml' path='docs/members[@name="TicketsController"]/UpdateTicket/*' />
        [HttpPut("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesProblem(StatusCodes.Status403Forbidden)]
        [ProducesProblem(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTicket(Guid id, [FromBody] UpdateTicketRequest request)
        {
            var command = new UpdateTicketCommand()
            {
                Id = id,
                UserId = _currentUserService.UserId,
                Title = request.Title,
                Description = request.Description
            };

            await _mediator.Send(command);

            return NoContent();
        }

        /// <include file='../Docs/TicketsDocs.xml' path='docs/members[@name="TicketsController"]/UpdateTicketStatus/*' />
        [Authorize]
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesProblem(StatusCodes.Status404NotFound)]
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

            return NoContent();
        }

        /// <include file='../Docs/TicketsDocs.xml' path='docs/members[@name="TicketsController"]/UpdateTicketPriority/*' />
        [HttpPatch("{id:guid}/priority")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesProblem(StatusCodes.Status403Forbidden)]
        [ProducesProblem(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTicketPriority(Guid id, [FromBody] UpdateTicketPriorityRequest request)
        {
            var command = new UpdateTicketPriorityCommand()
            {
                TicketId = id,
                NewPriority = request.Priority,
                UserId = _currentUserService.UserId
            };

            await _mediator.Send(command);

            return NoContent();
        }

        /// <include file='../Docs/TicketsDocs.xml' path='docs/members[@name="TicketsController"]/AssignAgent/*' />
        [HttpPatch("{id:guid}/assign")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesProblem(StatusCodes.Status403Forbidden)]
        [ProducesProblem(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignAgent(Guid id, [FromBody] AssignTicketRequest request)
        {
            var command = new AssignAgentCommand()
            {
                TicketId = id,
                AgentId = request.AgentId,
                UserId = _currentUserService.UserId
            };

            await _mediator.Send(command);

            return NoContent();
        }
    }
}
