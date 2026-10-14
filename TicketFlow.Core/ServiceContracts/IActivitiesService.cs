using TicketFlow.Core.DTO;

namespace TicketFlow.Core.ServiceContracts
{
    public interface IActivitiesService
    {
        Task<ApiResponse> GetActivitiesAsync(Guid ticketId, Guid userId, string role);
    }
}
