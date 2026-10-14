using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;

namespace TicketFlow.Infrastructure.Persistence.Repository
{
    public class TicketActivityRepository : GenericRepository<TicketActivity>, ITicketActivityRepository
    {
        public TicketActivityRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
