using MediatR;
using TicketFlow.Application.Features.Authentication.DTOs;

namespace TicketFlow.Application.Features.Authentication.Commands.Login
{
    public sealed class LoginCommand : IRequest<AuthenticationResponse>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
    }
}
