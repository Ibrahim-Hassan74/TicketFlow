using Dapper;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Users.DTOs;
using TicketFlow.Domain.Enums;
using TicketFlow.Infrastructure.Persistence.Models;

namespace TicketFlow.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// High-performance Dapper repository calling PostgreSQL stored functions for user queries.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public UserRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<PaginatedResponse<UserResponse>> GetUsersAsync(
            int page,
            int pageSize,
            UserRole? role,
            string? search,
            CancellationToken ct = default)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM sp_get_users(@Role, @Search, @Page, @PageSize);";

            var rows = (await connection.QueryAsync<UserRow>(
                new CommandDefinition(sql, new
                {
                    Role = role?.ToString(),
                    Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                    Page = page,
                    PageSize = pageSize
                }, cancellationToken: ct))).ToList();

            var totalCount = rows.Count > 0 ? (int)rows[0].TotalCount : 0;

            var items = rows.Select(r => new UserResponse
            {
                Id = r.Id,
                Email = r.Email,
                DisplayName = r.DisplayName,
                Role = r.Role,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt
            }).ToList();

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
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT 
                    u.""Id"",
                    COALESCE(u.""Email"", '') AS ""Email"",
                    u.""DisplayName"",
                    COALESCE(r.""Name"", '') AS ""Role"",
                    (NOT u.""IsDeleted"") AS ""IsActive"",
                    u.""CreatedAt""
                FROM ""AspNetUsers"" u
                LEFT JOIN ""AspNetUserRoles"" ur ON ur.""UserId"" = u.""Id""
                LEFT JOIN ""AspNetRoles"" r ON r.""Id"" = ur.""RoleId""
                WHERE u.""Id"" = @Id AND u.""IsDeleted"" = FALSE
                LIMIT 1;";

            return await connection.QueryFirstOrDefaultAsync<UserResponse>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        }

        public async Task<PaginatedResponse<UserResponse>> GetAgentsAsync(
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM sp_get_agents(@Page, @PageSize);";

            var rows = (await connection.QueryAsync<UserRow>(
                new CommandDefinition(sql, new
                {
                    Page = pageNumber,
                    PageSize = pageSize
                }, cancellationToken: ct))).ToList();

            var totalCount = rows.Count > 0 ? (int)rows[0].TotalCount : 0;

            var items = rows.Select(r => new UserResponse
            {
                Id = r.Id,
                Email = r.Email,
                DisplayName = r.DisplayName,
                Role = r.Role,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt
            }).ToList();

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
