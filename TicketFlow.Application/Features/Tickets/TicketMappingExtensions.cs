using TicketFlow.Application.Features.Tickets.DTOs;
using TicketFlow.Domain.Entities;

namespace TicketFlow.Application.Features.Tickets
{
    public static class TicketMappingExtensions
    {
        public static TicketResponse ToResponse(this Ticket ticket, string? customerName = null)
        {
            return new TicketResponse
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Priority = ticket.Priority.ToString(),
                Status = ticket.Status.ToString(),
                CustomerId = ticket.CustomerId,
                CustomerName = customerName ?? ticket.Customer?.DisplayName ?? string.Empty,
                AssignedAgentId = ticket.AssignedAgentId,
                AssignedAgentName = ticket.AssignedAgent?.DisplayName,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                ResolvedAt = ticket.ResolvedAt,
                ClosedAt = ticket.ClosedAt
            };
        }

        public static IEnumerable<TicketResponse> ToResponseList(this IEnumerable<Ticket> tickets)
        {
            return tickets.Select(t => t.ToResponse());
        }
    }
}
