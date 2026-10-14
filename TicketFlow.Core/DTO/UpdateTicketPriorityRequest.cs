using TicketFlow.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace TicketFlow.Core.DTO
{
    public class UpdateTicketPriorityRequest
    {
        [Required]
        public TicketPriority Priority { get; set; }
    }
}
