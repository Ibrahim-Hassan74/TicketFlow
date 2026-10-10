using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.Authentication.DTOs;
using TicketFlow.Application.Interfaces;
using TicketFlow.Domain.IdentityEntities;

namespace TicketFlow.Application.Features.Authentication.Commands.Login
{
    public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthenticationResponse>
    {
        private readonly IJwtService _jwtService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginCommandHandler> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoginCommandHandler(IJwtService jwtService, UserManager<ApplicationUser> userManager, ILogger<LoginCommandHandler> logger, SignInManager<ApplicationUser> signInManager)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
        }

        public async Task<AuthenticationResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found for email {Email}.", request.Email);
                throw new NotFoundException("User not found.");
            }

            if (user.IsDeleted)
            {
                _logger.LogWarning("Login failed: Account deactivated for user {UserId}.",user.Id);

                throw new AccountDeactivatedException("Your account has been deactivated. Please contact an administrator.");
            }

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserId} logged in successfully.", user.Id);
                var response = await _jwtService.CreateJwtToken(user, request.RememberMe);
                user.RefreshToken = response?.RefreshToken;
                user.RefreshTokenExpirationDateTime = response.RefreshTokenExpirationDateTime;
                await _userManager.UpdateAsync(user);
                return response;
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("Login failed: User {UserId} is locked out.", user.Id);

                throw new AccountLockedException("Your account is temporarily locked due to multiple failed login attempts. Please try again later.");
            }
            else if (result.IsNotAllowed)
            {
                _logger.LogWarning("Login failed: User {UserId} is not allowed to login.",user.Id);

                throw new LoginNotAllowedException("User is not allowed to login.");
            }
            else
            {
                _logger.LogWarning("Login failed: Invalid login attempt for user {UserId}.",user.Id);

                throw new InvalidCredentialsException("Incorrect email or password.");
            }
        }
    }
}
