using System.ComponentModel.DataAnnotations;

namespace TicketFlow.Core.DTO
{
    public class AssignTicketRequest
    {
        [Required]
        public Guid AgentId { get; set; }
    }
}
