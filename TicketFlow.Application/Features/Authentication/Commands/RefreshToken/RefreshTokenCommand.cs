using MediatR;
using TicketFlow.Application.Features.Authentication.DTOs;

namespace TicketFlow.Application.Features.Authentication.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<AuthenticationResponse>
    {
        public string? Token { get; set; } = string.Empty;
        public string? RefreshToken { get; set; } = string.Empty;
    }
}
