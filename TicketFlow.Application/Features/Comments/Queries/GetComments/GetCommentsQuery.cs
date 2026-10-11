using MediatR;
using TicketFlow.Application.Features.Comments.DTOs;

namespace TicketFlow.Application.Features.Comments.Queries.GetComments
{
    public class GetCommentsQuery : IRequest<IEnumerable<CommentResponse>>
    {
        public Guid TicketId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
