using MediatR;
using TicketFlow.Application.Features.Comments.DTOs;

namespace TicketFlow.Application.Features.Comments.Commands.AddComment
{
    public sealed class AddCommentCommand : IRequest<AddCommentResponse>
    {
        public string Content { get; set; } = string.Empty;
        public Guid TicketId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
