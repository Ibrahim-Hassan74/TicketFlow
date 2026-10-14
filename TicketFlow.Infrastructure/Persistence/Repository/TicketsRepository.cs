using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Contracts;

namespace TicketFlow.Infrastructure.Persistence.Repository
{
    public class TicketsRepository : GenericRepository<Ticket>, ITicketsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Ticket> _db;

        public TicketsRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
            _db = context.Set<Ticket>();
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
