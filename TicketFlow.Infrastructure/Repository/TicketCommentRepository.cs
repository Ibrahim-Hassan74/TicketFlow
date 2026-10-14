using TicketFlow.Core.Domain.Entities;
using TicketFlow.Core.RepositoryContracts;
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
