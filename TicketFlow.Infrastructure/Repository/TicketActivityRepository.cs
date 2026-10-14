using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Infrastructure.Data;

namespace TicketFlow.Infrastructure.Repository
{
    public class TicketActivityRepository : GenericRepository<TicketActivity>, ITicketActivityRepository
    {
        public TicketActivityRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
