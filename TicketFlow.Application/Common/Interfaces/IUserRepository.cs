using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Users.DTOs;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task<PaginatedResponse<UserResponse>> GetUsersAsync(int page, int pageSize, UserRole? role, string? search, CancellationToken ct = default);
        Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PaginatedResponse<UserResponse>> GetAgentsAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    }
}
