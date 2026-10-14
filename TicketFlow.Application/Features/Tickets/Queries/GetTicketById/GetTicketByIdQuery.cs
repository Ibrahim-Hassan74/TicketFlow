using MediatR;
using TicketFlow.Application.Features.Tickets.DTOs;

namespace TicketFlow.Application.Features.Tickets.Queries.GetTicketById
{
    public sealed class GetTicketByIdQuery : IRequest<TicketResponse>
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string Role { get; init; } = string.Empty;

        public GetTicketByIdQuery(Guid id, Guid userId, string role)
        {
            Id = id;
            UserId = userId;
            Role = role;
        }
    }
}
