using TicketFlow.Core.Domain.Entities;
using TicketFlow.Core.RepositoryContracts;
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
