using MediatR;
using Microsoft.Extensions.Logging;
using System.Data;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Activities.DTOs;
using TicketFlow.Domain.Contracts;

namespace TicketFlow.Application.Features.Activities.Queries.GetActivities
{
    public sealed class GetActivitiesQueryHandler : IRequestHandler<GetActivitiesQuery, IEnumerable<ActivityResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetActivitiesQueryHandler> _logger;
        private readonly ITicketAuthorizationService _ticketAuth;

        public GetActivitiesQueryHandler(IUnitOfWork unitOfWork, ILogger<GetActivitiesQueryHandler> logger, ITicketAuthorizationService ticketAuth)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _ticketAuth = ticketAuth;
        }

        public async Task<IEnumerable<ActivityResponse>> Handle(GetActivitiesQuery request, CancellationToken cancellationToken)
        {
            if (!await _ticketAuth.CanAccessTicketAsync(request.TicketId, request.UserId, request.Role, cancellationToken))
            {
                _logger.LogWarning("User {UserId} ({Role}) denied access to get activities for ticket {TicketId}.", request.UserId, request.Role, request.TicketId);
                throw new NotFoundException("Ticket not found or you do not have access.");
            }

            var activities = await _unitOfWork.TicketActivityRepository.GetActivitiesByTicketIdAsync(
                request.TicketId,
                cancellationToken);

            var response = activities.Select(a => new ActivityResponse
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
    }
}
