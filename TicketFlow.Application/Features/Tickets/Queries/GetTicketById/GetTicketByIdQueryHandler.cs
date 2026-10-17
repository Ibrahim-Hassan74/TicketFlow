using MediatR;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.Tickets.DTOs;
using TicketFlow.Domain.Contracts;

namespace TicketFlow.Application.Features.Tickets.Queries.GetTicketById
{
    public sealed class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, TicketResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTicketByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TicketResponse> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
        {
            var ticket = await _unitOfWork.TicketsRepository.GetByIdAsync(
                request.Id,
                cancellationToken,
                t => t.Customer,
                t => t.AssignedAgent!);

            if (ticket == null || !ticket.CanBeAccessedBy(request.UserId, request.Role))
            {
                throw new NotFoundException("Ticket not found.");
            }

            return ticket.ToResponse();
        }
    }
}
