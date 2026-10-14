using TicketFlow.Core.DTO;

namespace TicketFlow.Core.ServiceContracts
{
    public interface IUsersService
    {
        Task<ApiResponse> GetUsersAsync(UserQueryRequest request);

        Task<ApiResponse> GetUserByIdAsync(Guid id);

        Task<ApiResponse> CreateUserAsync(CreateUserRequest request);

        Task<ApiResponse> UpdateUserAsync(Guid id, UpdateUserRequest request);

        Task<ApiResponse> GetAgentsAsync(int pageNumber, int pageSize);
    }
}
