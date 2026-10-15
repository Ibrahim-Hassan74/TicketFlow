using Dapper;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.IdentityEntities;
using TicketFlow.Infrastructure.Persistence.Models;

namespace TicketFlow.Infrastructure.Persistence.Repository
{
    public class TicketActivityRepository : GenericRepository<TicketActivity>, ITicketActivityRepository
    {
        private readonly ISqlConnectionFactory? _connectionFactory;

        public TicketActivityRepository(ApplicationDbContext context, ISqlConnectionFactory? connectionFactory = null) : base(context)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<TicketActivity>> GetActivitiesByTicketIdAsync(Guid ticketId, CancellationToken ct = default)
        {
            if (_connectionFactory == null)
            {
                throw new InvalidOperationException("ISqlConnectionFactory is required for Dapper queries.");
            }

            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM sp_get_ticket_activities(@TicketId);";

            var rows = await connection.QueryAsync<TicketActivityRow>(
                new CommandDefinition(sql, new { TicketId = ticketId }, cancellationToken: ct));

            return rows.Select(r => new TicketActivity
            {
                Id = r.Id,
                TicketId = r.TicketId,
                UserId = r.UserId,
                User = new ApplicationUser { Id = r.UserId, DisplayName = r.UserName },
                Type = Enum.TryParse<ActivityType>(r.Type, out var parsedType) ? parsedType : ActivityType.Created,
                Description = r.Description,
                OldValue = r.OldValue,
                NewValue = r.NewValue,
                CreatedAt = r.CreatedAt
            }).ToList();
        }
    }
}
