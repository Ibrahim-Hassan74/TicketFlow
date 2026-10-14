using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Infrastructure.Data;

namespace TicketFlow.Infrastructure.Repository
{
    public class TimeEntryRepository : GenericRepository<TimeEntry>, ITimeEntryRepository
    {
        public TimeEntryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
