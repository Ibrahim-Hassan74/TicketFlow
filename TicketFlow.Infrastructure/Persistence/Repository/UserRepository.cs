using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Users.DTOs;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Infrastructure.Persistence.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResponse<UserResponse>> GetUsersAsync(
            int page,
            int pageSize,
            UserRole? role,
            string? search,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = from u in _context.Users.AsNoTracking()
                        join ur in _context.UserRoles on u.Id equals ur.UserId into urGroup
                        from ur in urGroup.DefaultIfEmpty()
                        join r in _context.Roles on ur.RoleId equals r.Id into rGroup
                        from r in rGroup.DefaultIfEmpty()
                        select new { User = u, Role = r != null ? r.Name : string.Empty };

            if (role.HasValue)
            {
                var roleName = role.Value.ToString();
                query = query.Where(x => x.Role == roleName);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.Trim().ToLower();
                query = query.Where(x =>
                    x.User.DisplayName.ToLower().Contains(searchLower) ||
                    (x.User.Email != null && x.User.Email.ToLower().Contains(searchLower)));
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(x => x.User.DisplayName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new UserResponse
                {
                    Id = x.User.Id,
                    Email = x.User.Email ?? string.Empty,
                    DisplayName = x.User.DisplayName,
                    Role = x.Role,
                    IsActive = !x.User.IsDeleted,
                    CreatedAt = x.User.CreatedAt
                })
                .ToListAsync(ct);

            return new PaginatedResponse<UserResponse>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var query = from u in _context.Users.AsNoTracking()
                        where u.Id == id
                        join ur in _context.UserRoles on u.Id equals ur.UserId into urGroup
                        from ur in urGroup.DefaultIfEmpty()
                        join r in _context.Roles on ur.RoleId equals r.Id into rGroup
                        from r in rGroup.DefaultIfEmpty()
                        select new UserResponse
                        {
                            Id = u.Id,
                            Email = u.Email ?? string.Empty,
                            DisplayName = u.DisplayName,
                            Role = r != null ? r.Name : string.Empty,
                            IsActive = !u.IsDeleted,
                            CreatedAt = u.CreatedAt
                        };

            return await query.FirstOrDefaultAsync(ct);
        }

        public async Task<PaginatedResponse<UserResponse>> GetAgentsAsync(
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var agentRoleName = UserRole.SupportAgent.ToString();

            var query = from u in _context.Users.AsNoTracking()
                        where !u.IsDeleted
                        join ur in _context.UserRoles on u.Id equals ur.UserId
                        join r in _context.Roles on ur.RoleId equals r.Id
                        where r.Name == agentRoleName
                        select new UserResponse
                        {
                            Id = u.Id,
                            Email = u.Email ?? string.Empty,
                            DisplayName = u.DisplayName,
                            Role = agentRoleName,
                            IsActive = !u.IsDeleted,
                            CreatedAt = u.CreatedAt
                        };

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PaginatedResponse<UserResponse>
            {
                Items = items,
                Page = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
