using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Domain.Contracts;

namespace TicketFlow.Infrastructure.Persistence.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public ITicketActivityRepository TicketActivityRepository { get; }
        public ITicketCommentRepository TicketCommentRepository { get; }

        public ITicketsRepository TicketsRepository { get; }

        public ITimeEntryRepository TimeEntryRepository { get; }

        public UnitOfWork(ApplicationDbContext context, ISqlConnectionFactory? connectionFactory = null)
        {
            _context = context;
            TicketActivityRepository = new TicketActivityRepository(_context, connectionFactory);
            TicketCommentRepository = new TicketCommentRepository(_context, connectionFactory);
            TicketsRepository = new TicketsRepository(_context, connectionFactory);
            TimeEntryRepository = new TimeEntryRepository(_context, connectionFactory);
        }


        public async Task<int> CompleteAsync(CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }
    }
}
