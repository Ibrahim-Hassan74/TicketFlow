using MediatR;
using Microsoft.Extensions.Logging;
using System.Data;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Comments.DTOs;
using TicketFlow.Domain.Contracts;

namespace TicketFlow.Application.Features.Comments.Queries.GetComments
{
    public class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, IEnumerable<CommentResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetCommentsQueryHandler> _logger;
        private readonly ITicketAuthorizationService _ticketAuth;

        public GetCommentsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetCommentsQueryHandler> logger, ITicketAuthorizationService ticketAuth)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _ticketAuth = ticketAuth;
        }

        public async Task<IEnumerable<CommentResponse>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
        {
            if (!await _ticketAuth.CanAccessTicketAsync(request.TicketId, request.UserId, request.Role, cancellationToken))
            {
                _logger.LogWarning("User {UserId} ({Role}) denied access to get comments for ticket {TicketId}.", request.UserId, request.Role, request.TicketId);
                throw new NotFoundException("Ticket not found or you do not have access.");
            }

            var comments = await _unitOfWork.TicketCommentRepository.GetFilteredAsync(
                c => c.TicketId == request.TicketId,
                c => c.User);

            var orderedComments = comments.OrderBy(c => c.CreatedAt).ToList();

            var response = orderedComments.Select(c => new CommentResponse
            {
                Id = c.Id,
                TicketId = c.TicketId,
                UserId = c.UserId,
                UserName = c.User?.DisplayName ?? "Unknown",
                Content = c.Content,
                CreatedAt = c.CreatedAt
            }).ToList();

            return response;
        }
    }
}
