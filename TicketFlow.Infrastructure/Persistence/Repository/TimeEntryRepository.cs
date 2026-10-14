using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;

namespace TicketFlow.Infrastructure.Persistence.Repository
{
    public class TimeEntryRepository : GenericRepository<TimeEntry>, ITimeEntryRepository
    {
        public TimeEntryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
