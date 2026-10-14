using MediatR;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.API.Attributes;
using Microsoft.AspNetCore.Authorization;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Activities.DTOs;
using TicketFlow.Application.Features.Activities.Queries.GetActivities;

namespace TicketFlow.API.Controllers
{
    /// <include file='../Docs/TicketActivitiesDocs.xml' path='docs/members[@name="TicketActivitiesController"]/Class/*' />
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/tickets/{ticketId:guid}/activities")]
    [Authorize]
    public class TicketActivitiesController : CustomControllerBase
    {
        private readonly ICurrentUserService _currentUserService;

        /// <include file='../Docs/TicketActivitiesDocs.xml' path='docs/members[@name="TicketActivitiesController"]/Constructor/*' />
        public TicketActivitiesController(IMediator mediator, ICurrentUserService currentUserService) : base(mediator)
        {
            _currentUserService = currentUserService;
        }

        /// <include file='../Docs/TicketActivitiesDocs.xml' path='docs/members[@name="TicketActivitiesController"]/GetActivities/*' />
        [HttpGet]
        [ProducesResponseType(typeof(List<ActivityResponse>), StatusCodes.Status200OK)]
        [ProducesProblem(StatusCodes.Status401Unauthorized)]
        [ProducesProblem(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActivities(Guid ticketId)
        {
            var query = new GetActivitiesQuery() { TicketId = ticketId, Role = _currentUserService.Role, UserId = _currentUserService.UserId };

            var response = await _mediator.Send(query);

            return Ok(response);
        }
    }
}
