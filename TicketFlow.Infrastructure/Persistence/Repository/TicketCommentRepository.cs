using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;

namespace TicketFlow.Infrastructure.Persistence.Repository
{
    public class TicketCommentRepository : GenericRepository<TicketComment>, ITicketCommentRepository
    {
        public TicketCommentRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
