using MediatR;
using Microsoft.Extensions.Logging;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Comments.DTOs;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Comments.Commands.AddComment
{
    public sealed class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, AddCommentResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddCommentCommandHandler> _logger;
        private readonly ITicketAuthorizationService _ticketAuth;

        public AddCommentCommandHandler(IUnitOfWork unitOfWork, ILogger<AddCommentCommandHandler> logger, ITicketAuthorizationService ticketAuth)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _ticketAuth = ticketAuth;
        }

        public async Task<AddCommentResponse> Handle(AddCommentCommand request, CancellationToken cancellationToken)
        {

            if (!await _ticketAuth.CanAccessTicketAsync(request.TicketId, request.UserId, request.Role, cancellationToken))
            {
                _logger.LogWarning("User {UserId} ({Role}) denied access to add comment to ticket {TicketId}.", request.UserId, request.Role, request.TicketId);
                throw new NotFoundException("Ticket not found or you do not have access.");
            }

            var comment = new TicketComment
            {
                TicketId = request.TicketId,
                UserId = request.UserId,
                Content = request.Content,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.TicketCommentRepository.AddAsync(comment, cancellationToken);

            var activity = new TicketActivity
            {
                TicketId = request.TicketId,
                UserId = request.UserId,
                Type = ActivityType.CommentAdded,
                Description = "A new comment was added.",
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _unitOfWork.TicketActivityRepository.AddAsync(activity, cancellationToken);

            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("User {UserId} added a comment to ticket {TicketId}.", request.UserId, request.TicketId);

            return new AddCommentResponse() { Message = "Comment added successfully." };
        }
    }
}
