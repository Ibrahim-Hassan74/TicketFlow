using MediatR;
using Asp.Versioning;
using TicketFlow.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.API.Attributes;
using Microsoft.AspNetCore.Authorization;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.TimeEntries.DTOs;
using TicketFlow.Application.Features.TimeEntries.Commands.AddTimeEntry;
using TicketFlow.Application.Features.TimeEntries.Queries.GetTimeEntries;

namespace TicketFlow.API.Controllers
{
    /// <include file='../Docs/TicketTimeEntriesDocs.xml' path='docs/members[@name="TicketTimeEntriesController"]/Class/*' />
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/tickets/{ticketId:guid}/time-entries")]
    [ProducesProblem(StatusCodes.Status401Unauthorized)]
    [ProducesProblem(StatusCodes.Status403Forbidden)]
    [ProducesProblem(StatusCodes.Status404NotFound)]
    public class TicketTimeEntriesController : CustomControllerBase
    {
        private readonly ICurrentUserService _currentUserService;

        /// <include file='../Docs/TicketTimeEntriesDocs.xml' path='docs/members[@name="TicketTimeEntriesController"]/Constructor/*' />
        public TicketTimeEntriesController(IMediator mediator, ICurrentUserService currentUserService) : base(mediator)
        {
            _currentUserService = currentUserService;
        }

        /// <include file='../Docs/TicketTimeEntriesDocs.xml' path='docs/members[@name="TicketTimeEntriesController"]/GetTimeEntries/*' />
        [HttpGet]
        [Authorize(Roles = $"{nameof(UserRole.SupportAgent)},{nameof(UserRole.Admin)}")]
        [ProducesResponseType(typeof(TicketTimeEntriesResponse), StatusCodes.Status200OK)]
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

        /// <include file='../Docs/TicketTimeEntriesDocs.xml' path='docs/members[@name="TicketTimeEntriesController"]/AddTimeEntry/*' />
        [HttpPost]
        [Authorize(Roles = nameof(UserRole.SupportAgent))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesProblem(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddTimeEntry(Guid ticketId, [FromBody] CreateTimeEntryRequest request)
        {
            var command = new AddTimeEntryCommand()
            {
                TicketId = ticketId,
                AgentId = _currentUserService.UserId,
                WorkDate = request.WorkDate,
                DurationMinutes = request.DurationMinutes,
                Description = request.Description
            };

            await _mediator.Send(command);

            return NoContent();
        }
    }
}
