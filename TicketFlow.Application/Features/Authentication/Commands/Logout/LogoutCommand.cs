using MediatR;

namespace TicketFlow.Application.Features.Authentication.Commands.Logout
{
    public sealed class LogoutCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
    }
}
