using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TicketFlow.Domain.Contracts;

namespace TicketFlow.Infrastructure.Persistence.Repository
{
    public class GenericRepository<TModel> : IGenericRepository<TModel> where TModel : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<TModel> _db;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _db = context.Set<TModel>();
        }
        /// <inheritdoc/>
        public async Task<TModel> AddAsync(TModel entity, CancellationToken ct = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
            }
            await _db.AddAsync(entity,ct);
            return entity;
        }

        /// <inheritdoc/>
        public async Task<int> CountAsync(CancellationToken ct = default)
        {
            return await _db.CountAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Id cannot be empty", nameof(id));
            }
            var entity = await _db.FindAsync([id], ct);
            if (entity == null)
            {
                return false;
            }
            _db.Remove(entity);
            return true;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TModel>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.AsNoTracking().ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TModel>> GetAllAsync(CancellationToken ct = default, params Expression<Func<TModel, object>>[] includes)
        {
            if (includes == null)
            {
                throw new ArgumentNullException(nameof(includes), "Includes cannot be null");
            }
            if (includes.Length == 0)
            {
                return await GetAllAsync(ct);
            }
            IQueryable<TModel> query = _db;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TModel>> GetFilteredAsync(Expression<Func<TModel, bool>> predicate, CancellationToken ct = default, params Expression<Func<TModel, object>>[] includes)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), "Predicate cannot be null");
            }
            
            IQueryable<TModel> query = _db.Where(predicate);
            
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            
            return await query.ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<TModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.FindAsync([id], ct);
        }

        /// <inheritdoc/>
        public async Task<TModel?> GetByIdAsync(Guid id, CancellationToken ct = default, params Expression<Func<TModel, object>>[] includes)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Id cannot be empty", nameof(id));
            }
            if (includes == null)
            {
                throw new ArgumentNullException(nameof(includes), "Includes cannot be null");
            }
            if (includes.Length == 0)
            {
                return await GetByIdAsync(id, ct);
            }
            IQueryable<TModel> query = _db;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, ct);
        }

        /// <inheritdoc/>
        public Task<TModel> UpdateAsync(TModel entity, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _context.Entry(entity).State = EntityState.Modified;
            return Task.FromResult(entity);
        }
    }
}
