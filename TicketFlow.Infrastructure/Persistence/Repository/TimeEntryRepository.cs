using Dapper;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.IdentityEntities;
using TicketFlow.Infrastructure.Persistence.Models;

namespace TicketFlow.Infrastructure.Persistence.Repository
{
    public class TimeEntryRepository : GenericRepository<TimeEntry>, ITimeEntryRepository
    {
        private readonly ISqlConnectionFactory? _connectionFactory;

        public TimeEntryRepository(ApplicationDbContext context, ISqlConnectionFactory? connectionFactory = null) : base(context)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<TimeEntry>> GetTimeEntriesByTicketIdAsync(Guid ticketId, CancellationToken ct = default)
        {
            if (_connectionFactory == null)
            {
                throw new InvalidOperationException("ISqlConnectionFactory is required for Dapper queries.");
            }

            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM sp_get_ticket_time_entries(@TicketId);";

            var rows = await connection.QueryAsync<TimeEntryRow>(
                new CommandDefinition(sql, new { TicketId = ticketId }, cancellationToken: ct));

            return rows.Select(r => new TimeEntry
            {
                Id = r.Id,
                TicketId = r.TicketId,
                AgentId = r.AgentId,
                Agent = new ApplicationUser { Id = r.AgentId, DisplayName = r.AgentName },
                WorkDate = r.WorkDate,
                DurationMinutes = r.DurationMinutes,
                Description = r.Description,
                CreatedAt = r.CreatedAt
            }).ToList();
        }
    }
}
