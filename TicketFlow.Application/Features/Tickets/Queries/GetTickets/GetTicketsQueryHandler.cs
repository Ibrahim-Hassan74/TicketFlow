using MediatR;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Tickets.DTOs;
using TicketFlow.Domain.Contracts;

namespace TicketFlow.Application.Features.Tickets.Queries.GetTickets
{
    public sealed class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, PaginatedResponse<TicketResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTicketsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResponse<TicketResponse>> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
        {
            var (tickets, totalCount) = await _unitOfWork.TicketsRepository.GetTicketsPagedAsync(
                request.Role,
                request.UserId,
                request.Status,
                request.Priority,
                request.Search,
                request.Page,
                request.PageSize,
                request.SortBy?.ToString(),
                request.SortOrder?.ToString(),
                cancellationToken);

            return new PaginatedResponse<TicketResponse>
            {
                Items = tickets.Select(t => t.ToResponse()).ToList(),
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
