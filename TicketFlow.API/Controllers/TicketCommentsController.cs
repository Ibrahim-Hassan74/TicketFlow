using MediatR;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.API.Attributes;
using Microsoft.AspNetCore.Authorization;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Comments.DTOs;
using TicketFlow.Application.Features.Comments.Commands.AddComment;
using TicketFlow.Application.Features.Comments.Queries.GetComments;

namespace TicketFlow.API.Controllers
{
    /// <include file='../Docs/TicketCommentsDocs.xml' path='docs/members[@name="TicketCommentsController"]/Class/*' />
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/tickets/{ticketId:guid}/comments")]
    [Authorize]
    [ProducesProblem(StatusCodes.Status401Unauthorized)]
    [ProducesProblem(StatusCodes.Status404NotFound)]
    public class TicketCommentsController : CustomControllerBase
    {
        private readonly ICurrentUserService _currentUserService;

        /// <include file='../Docs/TicketCommentsDocs.xml' path='docs/members[@name="TicketCommentsController"]/Constructor/*' />
        public TicketCommentsController(IMediator mediator, ICurrentUserService currentUserService) : base(mediator)
        {
            _currentUserService = currentUserService;
        }

        /// <include file='../Docs/TicketCommentsDocs.xml' path='docs/members[@name="TicketCommentsController"]/GetComments/*' />
        [HttpGet]
        [ProducesResponseType(typeof(List<CommentResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetComments(Guid ticketId)
        {
            var query = new GetCommentsQuery() { Role = _currentUserService.Role, UserId = _currentUserService.UserId, TicketId = ticketId };
            var response = await _mediator.Send(query);

            return Ok(response);
        }

        /// <include file='../Docs/TicketCommentsDocs.xml' path='docs/members[@name="TicketCommentsController"]/AddComment/*' />
        [HttpPost]
        [ProducesResponseType(typeof(AddCommentResponse), StatusCodes.Status200OK)]
        [ProducesProblem(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddComment(Guid ticketId, [FromBody] CreateCommentRequest request)
        {
            var command = new AddCommentCommand()
            {
                TicketId = ticketId,
                Role = _currentUserService.Role,
                UserId = _currentUserService.UserId,
                Content = request.Content
            };

            var response = await _mediator.Send(command);

            return Ok(response);
        }
    }
}
