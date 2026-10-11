using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Authentication.DTOs;
using TicketFlow.Domain.IdentityEntities;

namespace TicketFlow.Application.Features.Authentication.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthenticationResponse>
    {
        private readonly IJwtService _jwtService;
        private readonly UserManager<ApplicationUser> _userManager;

        public RefreshTokenCommandHandler(IJwtService jwtService, UserManager<ApplicationUser> userManager)
        {
            _jwtService = jwtService;
            _userManager = userManager;
        }

        public async Task<AuthenticationResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {

            ClaimsPrincipal? principal;

            try
            {
                principal = _jwtService.GetPrincipalFromJwtToken(request.Token);
            }
            catch (SecurityTokenException ex)
            {
                throw new InvalidTokenException("Access token is invalid.");
            }

            if (principal is null)
                throw new InvalidTokenException("Access token is invalid.");

            var email = principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
                throw new MissingTokenClaimException("Email claim is missing in token.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                throw new NotFoundException("User does not exist.");


            if (user.IsDeleted)
                throw new AccountDeactivatedException("Your account has been deactivated. Please contact an administrator.");

            if (user.RefreshToken != request.RefreshToken ||
                user.RefreshTokenExpirationDateTime <= DateTimeOffset.UtcNow)
            {
                throw new InvalidRefreshTokenException("Refresh token is invalid or expired.");
            }


            bool rememberMe = bool.TryParse(principal.FindFirst("remember_me")?.Value, out var rm) && rm;

            var authResponse = await _jwtService.CreateJwtToken(user, rememberMe);


            user.RefreshToken = authResponse?.RefreshToken;

            user.RefreshTokenExpirationDateTime = authResponse.RefreshTokenExpirationDateTime;

            await _userManager.UpdateAsync(user);

            return authResponse;
        }
    }
}
