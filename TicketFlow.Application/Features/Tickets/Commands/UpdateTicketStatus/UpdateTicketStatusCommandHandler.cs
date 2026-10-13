using MediatR;
using Microsoft.Extensions.Logging;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Tickets.Commands.UpdateTicketStatus
{
    public sealed class UpdateTicketStatusCommandHandler : IRequestHandler<UpdateTicketStatusCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateTicketStatusCommandHandler> _logger;
        public UpdateTicketStatusCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateTicketStatusCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Unit> Handle(UpdateTicketStatusCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _unitOfWork.TicketsRepository.GetByIdAsync(request.TicketId, cancellationToken);
            if (ticket == null)
                throw new NotFoundException("Ticket not found.");

            // Step 1: Check Ownership / Access by Role 
            ValidateUserAccess(ticket, request.UserId, request.Role);

            // Step 2: Validate Allowed Status Changes by Role 
            ValidateRolePermissions(ticket, request.NewStatus, request.Role);

            // Step 3: State Machine Transitions
            ValidateStateTransitions(ticket, request.NewStatus, request.UserId);

            // If status didn't change, nothing to do
            if (ticket.Status == request.NewStatus)
                return Unit.Value;

            // Step 4: Apply State Changes & Timestamps
            var oldStatus = ticket.Status;
            ticket.Status = request.NewStatus;
            ticket.UpdatedAt = DateTimeOffset.UtcNow;
            if (request.NewStatus == TicketStatus.Resolved)
                ticket.ResolvedAt = DateTimeOffset.UtcNow;
            if (request.NewStatus == TicketStatus.Closed)
                ticket.ClosedAt = DateTimeOffset.UtcNow;

            // Step 5: Log Activity
            var activity = new TicketActivity
            {
                TicketId = ticket.Id,
                UserId = request.UserId,
                Type = request.NewStatus == TicketStatus.Closed ? ActivityType.Closed : ActivityType.StatusChanged,
                Description = $"Status changed from {oldStatus} to {request.NewStatus}",
                OldValue = oldStatus.ToString(),
                NewValue = request.NewStatus.ToString(),
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.TicketActivityRepository.AddAsync(activity, cancellationToken);

            await _unitOfWork.TicketsRepository.UpdateAsync(ticket, cancellationToken);

            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("User {UserId} updated status for ticket {TicketId} from {OldStatus} to {NewStatus}.",
                request.UserId, request.TicketId, oldStatus, request.NewStatus);

            return Unit.Value;
        }
        private static void ValidateUserAccess(Ticket ticket, Guid userId, string role)
        {
            if (role == UserRole.Admin.ToString())
                return;
            if (role == UserRole.SupportAgent.ToString() && ticket.AssignedAgentId != userId)
                throw new NotFoundException("Ticket not found or not assigned to you.");
            if (role == UserRole.Customer.ToString() && ticket.CustomerId != userId)
                throw new NotFoundException("Ticket not found or does not belong to you.");
        }
        private static void ValidateRolePermissions(Ticket ticket, TicketStatus newStatus, string role)
        {
            if (role == UserRole.Admin.ToString())
                return; // Admins can set any status
            if (role == UserRole.Customer.ToString())
            {
                if (newStatus == TicketStatus.InProgress || newStatus == TicketStatus.Resolved)
                    throw new BadRequestException("Customers cannot set status to InProgress or Resolved.");
                // Customers can only reopen (Open) or close (Closed)
            }
            if (role == UserRole.SupportAgent.ToString())
            {
                if (newStatus == TicketStatus.Closed)
                    throw new BadRequestException("Support agents are not authorized to close tickets. Only customers or admins can close.");
            }
        }
        private void ValidateStateTransitions(Ticket ticket, TicketStatus newStatus, Guid userId)
        {
            if (ticket.Status == TicketStatus.Open && newStatus == TicketStatus.Resolved)
            {
                _logger.LogWarning("User {UserId} attempted invalid transition for ticket {TicketId}: Open to Resolved.", userId, ticket.Id);
                throw new BadRequestException("Cannot resolve an open ticket. Must be In Progress first.");
            }
            if (ticket.Status == TicketStatus.Closed && newStatus != TicketStatus.Open)
            {
                _logger.LogWarning("User {UserId} attempted invalid transition for ticket {TicketId}: Closed to {NewStatus}.", userId, ticket.Id, newStatus);
                throw new BadRequestException("A closed ticket can only be reopened (set to Open).");
            }
        }
    }
}
