using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.Tickets.DTOs;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.IdentityEntities;

namespace TicketFlow.Application.Features.Tickets.Commands.CreateTicket
{
    public sealed class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, TicketResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CreateTicketCommandHandler> _logger;
        public CreateTicketCommandHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, ILogger<CreateTicketCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<TicketResponse> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
        {
            var customer = await _userManager.FindByIdAsync(request.CustomerId.ToString());
            if (customer == null)
                throw new NotFoundException("Customer not found.");

            var ticket = new Ticket
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                Status = TicketStatus.Open,
                CustomerId = request.CustomerId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            var createdTicket = await _unitOfWork.TicketsRepository.AddAsync(ticket, cancellationToken);

            var activity = new TicketActivity
            {
                TicketId = createdTicket.Id,
                UserId = request.CustomerId,
                Type = ActivityType.Created,
                Description = "Ticket created.",
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _unitOfWork.TicketActivityRepository.AddAsync(activity, cancellationToken);

            await _unitOfWork.CompleteAsync(cancellationToken);

            var response = createdTicket.ToResponse(customer.DisplayName); ;

            _logger.LogInformation("Customer {CustomerId} created ticket {TicketId}.", request.CustomerId, createdTicket.Id);

            return response;
        }
    }
}
