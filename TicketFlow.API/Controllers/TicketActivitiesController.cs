using MediatR;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Activities.DTOs;
using TicketFlow.Application.Features.Activities.Queries.GetActivities;

namespace TicketFlow.API.Controllers
{
    /// <summary>
    /// Controller responsible for retrieving the activity timeline (audit trail) for a specific ticket.
    /// </summary>
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/tickets/{ticketId:guid}/activities")]
    [Authorize]
    public class TicketActivitiesController : CustomControllerBase
    {
        private readonly ICurrentUserService _currentUserService;

        public TicketActivitiesController(IMediator mediator, ICurrentUserService currentUserService) : base(mediator)
        {
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Retrieves the chronological history of activities for a specific ticket.
        /// Users can only view the activity timeline for tickets they have access to.
        /// </summary>
        /// <param name="ticketId">The unique identifier of the ticket.</param>
        /// <returns>A list of activities representing the audit trail of the ticket.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ActivityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActivities(Guid ticketId)
        {
            var query = new GetActivitiesQuery() { TicketId = ticketId, Role = _currentUserService.Role, UserId = _currentUserService.UserId };

            var response = await _mediator.Send(query);

            return Ok(response);
        }
    }
}
