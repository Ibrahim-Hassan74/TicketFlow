using MediatR;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Comments.DTOs;
using TicketFlow.Application.Features.Comments.Commands.AddComment;
using TicketFlow.Application.Features.Comments.Queries.GetComments;

namespace TicketFlow.API.Controllers
{
    /// <summary>
    /// Controller responsible for managing comments on a specific support ticket.
    /// Provides endpoints for retrieving and adding comments.
    /// </summary>
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/tickets/{ticketId:guid}/comments")]
    [Authorize]
    public class TicketCommentsController : CustomControllerBase
    {
        private readonly ICurrentUserService _currentUserService;

        public TicketCommentsController(IMediator mediator, ICurrentUserService currentUserService) : base(mediator)
        {
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Retrieves a chronological list of comments for a specific ticket.
        /// Users can only view comments for tickets they have access to.
        /// </summary>
        /// <param name="ticketId">The unique identifier of the ticket.</param>
        /// <returns>A list of comments associated with the ticket.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<CommentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetComments(Guid ticketId)
        {
            var query = new GetCommentsQuery() { Role = _currentUserService.Role, UserId = _currentUserService.UserId, TicketId = ticketId };
            var response = await _mediator.Send(query);

            return Ok(response);
        }

        /// <summary>
        /// Adds a new comment to a specific ticket.
        /// </summary>
        /// <param name="ticketId">The unique identifier of the ticket.</param>
        /// <param name="request">The content of the new comment.</param>
        /// <returns>A success response if the comment is added.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(AddCommentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
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
