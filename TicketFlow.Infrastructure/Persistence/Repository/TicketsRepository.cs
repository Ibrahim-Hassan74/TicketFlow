using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.IdentityEntities;
using TicketFlow.Infrastructure.Persistence.Models;

namespace TicketFlow.Infrastructure.Persistence.Repository
{
    public class TicketsRepository : GenericRepository<Ticket>, ITicketsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Ticket> _db;
        private readonly ISqlConnectionFactory? _connectionFactory;

        public TicketsRepository(ApplicationDbContext context, ISqlConnectionFactory? connectionFactory = null) : base(context)
        {
            _context = context;
            _db = context.Set<Ticket>();
            _connectionFactory = connectionFactory;
        }

        public async Task<(IEnumerable<Ticket> Items, int TotalCount)> GetTicketsPagedAsync(
            string? role,
            Guid userId,
            TicketStatus? status,
            TicketPriority? priority,
            string? search,
            int page,
            int pageSize,
            string? sortBy,
            string? sortOrder,
            CancellationToken ct = default)
        {
            if (_connectionFactory == null)
            {
                throw new InvalidOperationException("ISqlConnectionFactory is required for Dapper queries.");
            }

            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM fn_get_tickets(@Role, @UserId, @Status, @Priority, @Search, @Page, @PageSize, @SortBy, @SortOrder);";

            var rows = (await connection.QueryAsync<TicketRow>(
                new CommandDefinition(sql, new
                {
                    Role = role,
                    UserId = userId,
                    Status = (int?)status,
                    Priority = (int?)priority,
                    Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                    Page = page,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortOrder = sortOrder
                }, cancellationToken: ct))).ToList();

            var totalCount = rows.Count > 0 ? (int)rows[0].TotalCount : 0;

            var items = rows.Select(r => new Ticket
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                Status = (TicketStatus)r.Status,
                Priority = (TicketPriority)r.Priority,
                CustomerId = r.CustomerId,
                Customer = new ApplicationUser { Id = r.CustomerId, DisplayName = r.CustomerName },
                AssignedAgentId = r.AssignedAgentId,
                AssignedAgent = r.AssignedAgentId.HasValue
                    ? new ApplicationUser { Id = r.AssignedAgentId.Value, DisplayName = r.AssignedAgentName ?? string.Empty }
                    : null,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                ResolvedAt = r.ResolvedAt,
                ClosedAt = r.ClosedAt
            }).ToList();

            return (items, totalCount);
        }

        public async Task<int> CountFilteredTicketsAsync(Expression<Func<Ticket, bool>>? predicate, CancellationToken ct = default)
        {
            IQueryable<Ticket> query = _db;
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            return await query.CountAsync(ct);
        }

        public async Task<IEnumerable<Ticket>> GetFilteredTicketsAsync(
            Expression<Func<Ticket, bool>>? predicate, 
            Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>>? orderBy, 
            int pageNumber, 
            int pageSize, 
            CancellationToken ct = default,
            params Expression<Func<Ticket, object>>[] includes)
        {
            IQueryable<Ticket> query = _db;

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(ct);
        }
    }
}
