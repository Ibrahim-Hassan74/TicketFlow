using MediatR;
using Asp.Versioning;
using TicketFlow.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.API.Attributes;
using Microsoft.AspNetCore.Authorization;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Users.DTOs;
using TicketFlow.Application.Features.Users.Queries.GetUsers;
using TicketFlow.Application.Features.Users.Queries.GetAgents;
using TicketFlow.Application.Features.Users.Commands.CreateUser;
using TicketFlow.Application.Features.Users.Commands.UpdateUser;
using TicketFlow.Application.Features.Users.Queries.GetUserById;

namespace TicketFlow.API.Controllers
{
    /// <include file='../Docs/UsersDocs.xml' path='docs/members[@name="UsersController"]/Class/*' />
    [ApiVersion(1.0)]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesProblem(StatusCodes.Status401Unauthorized)]
    [ProducesProblem(StatusCodes.Status403Forbidden)]
    public class UsersController : CustomControllerBase
    {
        /// <include file='../Docs/UsersDocs.xml' path='docs/members[@name="UsersController"]/Constructor/*' />
        public UsersController(IMediator mediator) : base(mediator)
        {
        }

        /// <include file='../Docs/UsersDocs.xml' path='docs/members[@name="UsersController"]/GetUsers/*' />
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesProblem(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        /// <include file='../Docs/UsersDocs.xml' path='docs/members[@name="UsersController"]/GetUserById/*' />
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesProblem(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var query = new GetUserByIdQuery()
            {
                Id = id
            };

            var response = await _mediator.Send(query);

            return Ok(response);
        }

        /// <include file='../Docs/UsersDocs.xml' path='docs/members[@name="UsersController"]/CreateUser/*' />
        [HttpPost]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesProblem(StatusCodes.Status400BadRequest)]
        [ProducesProblem(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            var response = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetUserById), new { id = response.Id }, response);
        }

        /// <include file='../Docs/UsersDocs.xml' path='docs/members[@name="UsersController"]/UpdateUser/*' />
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesProblem(StatusCodes.Status400BadRequest)]
        [ProducesProblem(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
        {
            var command = new UpdateUserCommand()
            {
                Id = id,
                UpdateUser = request
            };

            var response = await _mediator.Send(command);

            return Ok(response);
        }

        /// <include file='../Docs/UsersDocs.xml' path='docs/members[@name="UsersController"]/GetAgents/*' />
        [HttpGet("agents")]
        [ProducesResponseType(typeof(PaginatedResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesProblem(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAgents([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAgentsQuery()
            {
                Page = pageNumber,
                PageSize = pageSize
            };

            var response = await _mediator.Send(query);

            return Ok(response);
        }
    }
}
