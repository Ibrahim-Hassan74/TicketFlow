using MediatR;
using Asp.Versioning;
using TicketFlow.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
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
    /// <summary>
    /// Controller responsible for managing users in the Support Ticket Management system.
    /// Provides endpoints to retrieve, create, and update user accounts.
    /// Only accessible to users with the Admin role.
    /// </summary>
    [ApiVersion(1.0)]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class UsersController : CustomControllerBase
    {

        public UsersController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Retrieves a paginated list of users based on the specified query parameters.
        /// </summary>
        /// <param name="query">
        /// The query parameters for filtering, sorting, and paginating the user list.
        /// </param>
        /// <returns>
        /// Returns a paginated response containing a list of users.
        /// </returns>
        /// <response code="200">
        /// Successfully retrieved the list of users. Returns <see cref="PaginatedResponse{UserResponse}"/>.
        /// </response>
        /// <response code="400">
        /// Bad Request – Invalid query parameters. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="401">
        /// Unauthorized – The user is not authenticated.
        /// </response>
        /// <response code="403">
        /// Forbidden – The user does not have the Admin role.
        /// </response>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        /// <summary>
        /// Retrieves a specific user by their unique identifier.
        /// </summary>
        /// <param name="id">
        /// The unique identifier (GUID) of the user.
        /// </param>
        /// <returns>
        /// Returns the user details if found.
        /// </returns>
        /// <response code="200">
        /// Successfully retrieved the user details. Returns <see cref="UserResponse"/>.
        /// </response>
        /// <response code="401">
        /// Unauthorized – The user is not authenticated.
        /// </response>
        /// <response code="403">
        /// Forbidden – The user does not have the Admin role.
        /// </response>
        /// <response code="404">
        /// Not Found – The user with the specified ID does not exist. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var query = new GetUserByIdQuery()
            {
                Id = id
            };

            var response = await _mediator.Send(query);

            return Ok(response);
        }

        /// <summary>
        /// Creates a new user account in the system.
        /// </summary>
        /// <param name="command">
        /// The details of the user to create, including their role and contact information.
        /// </param>
        /// <returns>
        /// Returns the details of the newly created user.
        /// </returns>
        /// <response code="200">
        /// Successfully created the user. Returns <see cref="UserResponse"/>.
        /// </response>
        /// <response code="400">
        /// Bad Request – Invalid input data (e.g., missing fields or invalid email). Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="401">
        /// Unauthorized – The user is not authenticated.
        /// </response>
        /// <response code="403">
        /// Forbidden – The user does not have the Admin role.
        /// </response>
        /// <response code="409">
        /// Conflict – A user with the same email already exists. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        [HttpPost]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            var response = await _mediator.Send(command);

            return Ok(response);
        }

        /// <summary>
        /// Updates an existing user's details.
        /// </summary>
        /// <param name="id">
        /// The unique identifier (GUID) of the user to update.
        /// </param>
        /// <param name="request">
        /// The updated details for the user.
        /// </param>
        /// <returns>
        /// Returns the updated user details.
        /// </returns>
        /// <response code="200">
        /// Successfully updated the user. Returns <see cref="UserResponse"/>.
        /// </response>
        /// <response code="400">
        /// Bad Request – Invalid input data. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="401">
        /// Unauthorized – The user is not authenticated.
        /// </response>
        /// <response code="403">
        /// Forbidden – The user does not have the Admin role.
        /// </response>
        /// <response code="404">
        /// Not Found – The user with the specified ID does not exist. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Retrieves a paginated list of users with the Agent role.
        /// </summary>
        /// <param name="pageNumber">
        /// The page number to retrieve (defaults to 1).
        /// </param>
        /// <param name="pageSize">
        /// The number of agents per page (defaults to 10).
        /// </param>
        /// <returns>
        /// Returns a paginated response containing a list of agents.
        /// </returns>
        /// <response code="200">
        /// Successfully retrieved the list of agents. Returns <see cref="PaginatedResponse{UserResponse}"/>.
        /// </response>
        /// <response code="400">
        /// Bad Request – Invalid pagination parameters. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="401">
        /// Unauthorized – The user is not authenticated.
        /// </response>
        /// <response code="403">
        /// Forbidden – The user does not have the Admin role.
        /// </response>
        [HttpGet("agents")]
        [ProducesResponseType(typeof(PaginatedResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
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
