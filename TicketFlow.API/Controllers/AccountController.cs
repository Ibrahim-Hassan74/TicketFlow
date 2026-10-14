using MediatR;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.API.Attributes;
using Microsoft.AspNetCore.Authorization;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Users.DTOs;
using TicketFlow.Application.Features.Authentication.DTOs;
using TicketFlow.Application.Features.Users.Queries.GetUserById;
using TicketFlow.Application.Features.Authentication.Commands.Login;
using TicketFlow.Application.Features.Authentication.Commands.Logout;
using TicketFlow.Application.Features.Authentication.Commands.Register;
using TicketFlow.Application.Features.Authentication.Commands.RefreshToken;

namespace TicketFlow.API.Controllers
{
    /// <include file='../Docs/AccountDocs.xml' path='docs/members[@name="AccountController"]/Class/*' />
    [ApiVersion(1.0)]
    [ProducesValidationProblem]
    [ProducesProblem(StatusCodes.Status404NotFound)]
    public class AccountController : CustomControllerBase
    {
        private readonly ICurrentUserService _currentUserService;

        /// <include file='../Docs/AccountDocs.xml' path='docs/members[@name="AccountController"]/Constructor/*' />
        public AccountController(IMediator mediator, ICurrentUserService currentUserService) : base(mediator)
        {
            _currentUserService = currentUserService;
        }

        /// <include file='../Docs/AccountDocs.xml' path='docs/members[@name="AccountController"]/PostRegister/*' />
        [HttpPost("register")]
        [Authorize("NotAuthorized")]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
        [ProducesProblem(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PostRegister([FromBody] RegisterCommand command)
        {
            var response = await _mediator.Send(command);

            return Ok(response);
        }

        /// <include file='../Docs/AccountDocs.xml' path='docs/members[@name="AccountController"]/PostLogin/*' />
        [HttpPost("login")]
        [Authorize("NotAuthorized")]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesProblem(StatusCodes.Status401Unauthorized)]
        [ProducesProblem(StatusCodes.Status403Forbidden)]
        [ProducesProblem(StatusCodes.Status404NotFound)]
        [ProducesProblem(StatusCodes.Status423Locked)]
        public async Task<IActionResult> PostLogin([FromBody] LoginCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        /// <include file='../Docs/AccountDocs.xml' path='docs/members[@name="AccountController"]/RefreshToken/*' />
        [HttpPost("generate-new-jwt-token")]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesProblem(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        /// <include file='../Docs/AccountDocs.xml' path='docs/members[@name="AccountController"]/PostLogout/*' />
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesProblem(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PostLogout()
        {
            var Id = _currentUserService.UserId;

            var command = new LogoutCommand() { Id = Id };

            var response = await _mediator.Send(command);

            return NoContent();
        }

        /// <include file='../Docs/AccountDocs.xml' path='docs/members[@name="AccountController"]/GetCurrentUser/*' />
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesProblem(StatusCodes.Status404NotFound)]
        [ProducesProblem(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = _currentUserService.UserId;

            var command = new GetUserByIdQuery() { Id = userId };

            var response = await _mediator.Send(command);

            return Ok(response);
        }
    }
}
