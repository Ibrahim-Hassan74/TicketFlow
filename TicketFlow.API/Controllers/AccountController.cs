using MediatR;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Users.DTOs;
using TicketFlow.Application.Features.Authentication.DTOs;
using TicketFlow.Application.Features.Users.Queries.GetUserById;
using TicketFlow.Application.Features.Authentication.Commands.Login;
using TicketFlow.Application.Features.Authentication.Commands.Logout;
using TicketFlow.Application.Features.Authentication.Commands.RefreshToken;
using TicketFlow.Application.Features.Authentication.Commands.Register;

namespace TicketFlow.API.Controllers
{
    /// <summary>
    /// Controller responsible for handling user authentication-related actions
    /// such as registration, login, email confirmation, and password reset for the Support Ticket Management system.
    /// </summary>
    [ApiVersion(1.0)]
    public class AccountController : CustomControllerBase
    {
        private readonly ICurrentUserService _currentUserService;

        public AccountController(IMediator mediator, ICurrentUserService currentUserService) : base(mediator)
        {
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="command">
        /// An object containing user registration details like username, email, and password.
        /// </param>
        /// <returns>
        /// Returns a response depending on the outcome of the registration process.
        /// </returns>
        /// <response code="200">Registration succeeded. The user account has been created successfully.</response>
        /// <response code="400">
        /// Registration failed due to invalid input.
        /// For example:
        /// - Missing required fields (username, email, or password)
        /// - Password does not meet security requirements
        /// - Email format is invalid
        /// </response>
        /// <response code="409">
        /// Registration failed because the email or username is already in use.
        /// The system does not allow duplicate accounts with the same email.
        /// </response>
        [HttpPost("register")]
        [Authorize("NotAuthorized")]
        [ProducesResponseType(typeof(RegisterCommand), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PostRegister([FromBody] RegisterCommand command)
        {
            var response = await _mediator.Send(command);

            return Ok(response);
        }

        /// <summary>
        /// Authenticates an existing user and generates a JWT token.
        /// </summary>
        /// <param name="command">
        /// The user's login credentials (email and password).
        /// </param>
        /// <returns>
        /// Returns a response based on the login outcome.
        /// </returns>
        /// <response code="200">
        /// Login succeeded. Returns <see cref="AuthenticationResponse"/> with a JWT token and refresh token.
        /// </response>
        /// <response code="400">
        /// Bad Request – Input invalid. Examples include:
        /// - loginDTO is null
        /// - Email or password missing
        /// - Invalid input format
        /// Returns <see cref="ApiErrorResponse"/> with details.
        /// </response>
        /// <response code="401">
        /// Unauthorized – Invalid credentials. 
        /// The email/password combination does not match any user account.
        /// Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="404">
        /// Not Found – User does not exist with the provided email.
        /// Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="423">
        /// Locked – Account temporarily locked due to multiple failed login attempts.
        /// Returns <see cref="ApiErrorResponse"/> explaining the lockout period.
        /// </response>
        [HttpPost("login")]
        [Authorize("NotAuthorized")]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status423Locked)]
        public async Task<IActionResult> PostLogin([FromBody] LoginCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        /// <summary>
        /// Generates a new access token (and refresh token) using a valid refresh token.
        /// </summary>
        /// <param name="command">
        /// Contains the expired access token and its corresponding refresh token.
        /// </param>
        /// <returns>
        /// Returns a response indicating whether the token refresh succeeded.
        /// </returns>
        /// <response code="200">
        /// Token refreshed successfully. Returns <see cref="AuthenticationResponse"/> containing the new JWT and refresh token.
        /// </response>
        /// <response code="400">
        /// Bad Request – Input is invalid, missing, or the refresh token is expired/does not match the user. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="404">
        /// Not Found – User associated with the token does not exist. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        [HttpPost("generate-new-jwt-token")]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        /// <summary>
        /// Logs out the currently authenticated user by clearing their refresh token and signing them out.
        /// </summary>
        /// <remarks>
        /// Requires the user to be authenticated. The user's email is extracted from the JWT claims
        /// to identify the account to log out. Refresh tokens are cleared and the user is signed out.
        /// </remarks>
        /// <returns>
        /// Returns a success message if logout succeeds, or an unauthorized response if the user is not authenticated.
        /// </returns>
        /// <response code="200">
        /// Logout successful. Returns <see cref="ApiResponse"/>.
        /// </response>
        /// <response code="401">
        /// Unauthorized – The user is not authenticated. Returns <see cref="ApiResponse"/>.
        /// </response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PostLogout()
        {
            var Id = _currentUserService.UserId;

            var command = new LogoutCommand() { Id = Id };

            var response = await _mediator.Send(command);

            return Ok();
        }

        /// <summary>
        /// Retrieves information about the currently authenticated user.
        /// </summary>
        /// <remarks>
        /// Requires the user to be authenticated. The endpoint reads the user ID from the JWT claims
        /// and fetches the corresponding user details from the authentication service.
        /// </remarks>
        /// <returns>
        /// Returns detailed information about the authenticated user.
        /// </returns>
        /// <response code="200">
        /// User found and returned successfully. Returns <see cref="UserResponse"/>.
        /// </response>
        /// <response code="400">
        /// Bad Request – No user ID found in claims. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="404">
        /// Not Found – User does not exist. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        /// <response code="401">
        /// Unauthorized – The user is not authenticated. Returns <see cref="ApiErrorResponse"/>.
        /// </response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = _currentUserService.UserId;

            var command = new GetUserByIdQuery() { Id = userId };

            var response = await _mediator.Send(command);

            return Ok(response);
        }
    }
}
