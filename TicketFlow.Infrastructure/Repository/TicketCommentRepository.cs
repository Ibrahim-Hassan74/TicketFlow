using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Infrastructure.Data;

namespace TicketFlow.Infrastructure.Repository
{
    public class TicketCommentRepository : GenericRepository<TicketComment>, ITicketCommentRepository
    {
        public TicketCommentRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
