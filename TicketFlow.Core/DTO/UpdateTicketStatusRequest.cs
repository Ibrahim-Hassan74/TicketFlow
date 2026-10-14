using TicketFlow.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace TicketFlow.Core.DTO
{
    public class UpdateTicketStatusRequest
    {
        [Required]
        public TicketStatus Status { get; set; }
    }
}
