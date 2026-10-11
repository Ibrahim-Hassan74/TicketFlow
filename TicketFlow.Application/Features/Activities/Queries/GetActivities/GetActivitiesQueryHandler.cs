using MediatR;
using Microsoft.Extensions.Logging;
using System.Data;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.Activities.DTOs;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Activities.Queries.GetActivities
{
    public sealed class GetActivitiesQueryHandler : IRequestHandler<GetActivitiesQuery, IEnumerable<ActivityResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetActivitiesQueryHandler> _logger;

        public GetActivitiesQueryHandler(IUnitOfWork unitOfWork, ILogger<GetActivitiesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<ActivityResponse>> Handle(GetActivitiesQuery request, CancellationToken cancellationToken)
        {
            if (!await CanAccessTicketAsync(request.TicketId, request.UserId, request.Role))
            {
                _logger.LogWarning("User {UserId} ({Role}) denied access to get activities for ticket {TicketId}.", request.TicketId, request.UserId, request.Role);
                throw new NotFoundException("Ticket not found or you do not have access.");
            }

            var activities = await _unitOfWork.TicketActivityRepository.GetFilteredAsync(
                a => a.TicketId == request.TicketId,
                a => a.User);

            var orderedActivities = activities.OrderBy(a => a.CreatedAt).ToList();

            var response = orderedActivities.Select(a => new ActivityResponse
            {
                Id = a.Id,
                TicketId = a.TicketId,
                UserId = a.UserId,
                UserName = a.User?.DisplayName ?? "Unknown",
                Type = a.Type.ToString(),
                Description = a.Description,
                OldValue = a.OldValue,
                NewValue = a.NewValue,
                CreatedAt = a.CreatedAt
            }).ToList();

            return response;
        }

        private async Task<bool> CanAccessTicketAsync(Guid ticketId, Guid userId, string role)
        {
            var ticket = await _unitOfWork.TicketsRepository.GetByIdAsync(ticketId);
            if (ticket == null) return false;

            if (role == UserRole.Admin.ToString())
                return true;

            if (role == UserRole.SupportAgent.ToString())
                return ticket.AssignedAgentId == userId;

            if (role == UserRole.Customer.ToString())
                return ticket.CustomerId == userId;

            return false;
        }
    }
}
