using MediatR;
using System.Linq.Expressions;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Tickets.DTOs;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;

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
            // 1. Determine Role Scope Filter
            // Admin: sees all tickets (null)
            // Agent: sees only tickets assigned to them
            // Customer: sees only tickets they created
            Expression<Func<Ticket, bool>>? rolePredicate = request.Role switch
            {
                nameof(UserRole.SupportAgent) => t => t.AssignedAgentId == request.UserId,
                nameof(UserRole.Customer) => t => t.CustomerId == request.UserId,
                _ => null
            };

            // 2. Determine Search & Status Filter

            Expression<Func<Ticket, bool>>? filterPredicate = null;
            if (request.Status.HasValue || request.Priority.HasValue || !string.IsNullOrWhiteSpace(request.Search))
            {
                filterPredicate = t =>
                    (!request.Status.HasValue || t.Status == request.Status.Value) &&
                    (!request.Priority.HasValue || t.Priority == request.Priority.Value) &&
                    (string.IsNullOrWhiteSpace(request.Search) || t.Title.Contains(request.Search));
            }

            // 3. Combine Predicates (Role AND Filter)
            var finalPredicate = CombinePredicates(rolePredicate, filterPredicate);

            // 4. Sorting
            Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>> orderBy = BuildOrderBy(request.SortBy, request.SortOrder);

            // 5. Execute Repository Queries

            var tickets = await _unitOfWork.TicketsRepository.GetFilteredTicketsAsync(
                finalPredicate,
                orderBy,
                request.Page,
                request.PageSize,
                cancellationToken,
                t => t.Customer,
                t => t.AssignedAgent!);

            var totalCount = await _unitOfWork.TicketsRepository.CountFilteredTicketsAsync(finalPredicate, cancellationToken);

            return new PaginatedResponse<TicketResponse>
            {
                Items = tickets.Select(t => t.ToResponse()).ToList(),
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }

        private static Expression<Func<Ticket, bool>>? CombinePredicates(Expression<Func<Ticket, bool>>? first, Expression<Func<Ticket, bool>>? second)
        {
            if (first == null) return second;
            if (second == null) return first;
            var parameter = Expression.Parameter(typeof(Ticket), "t");
            var combined = Expression.AndAlso(
                Expression.Invoke(first, parameter),
                Expression.Invoke(second, parameter));
            return Expression.Lambda<Func<Ticket, bool>>(combined, parameter);
        }

        private static Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>> BuildOrderBy(SortByOptions? sortBy, SortOrderOptions? sortOrder)
        {
            bool isDesc = sortOrder == SortOrderOptions.Descending;
            return sortBy switch
            {
                SortByOptions.Priority => q => isDesc ? q.OrderByDescending(t => t.Priority) : q.OrderBy(t => t.Priority),
                SortByOptions.Status => q => isDesc ? q.OrderByDescending(t => t.Status) : q.OrderBy(t => t.Status),
                SortByOptions.UpdatedAt => q => isDesc ? q.OrderByDescending(t => t.UpdatedAt) : q.OrderBy(t => t.UpdatedAt),
                _ => q => isDesc ? q.OrderByDescending(t => t.CreatedAt) : q.OrderBy(t => t.CreatedAt)
            };
        }
    }
}
