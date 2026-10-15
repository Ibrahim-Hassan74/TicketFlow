using Dapper;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.IdentityEntities;
using TicketFlow.Infrastructure.Persistence.Models;

namespace TicketFlow.Infrastructure.Persistence.Repository
{
    public class TicketCommentRepository : GenericRepository<TicketComment>, ITicketCommentRepository
    {
        private readonly ISqlConnectionFactory? _connectionFactory;

        public TicketCommentRepository(ApplicationDbContext context, ISqlConnectionFactory? connectionFactory = null) : base(context)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<TicketComment>> GetCommentsByTicketIdAsync(Guid ticketId, CancellationToken ct = default)
        {
            if (_connectionFactory == null)
            {
                throw new InvalidOperationException("ISqlConnectionFactory is required for Dapper queries.");
            }

            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM sp_get_ticket_comments(@TicketId);";

            var rows = await connection.QueryAsync<TicketCommentRow>(
                new CommandDefinition(sql, new { TicketId = ticketId }, cancellationToken: ct));

            return rows.Select(r => new TicketComment
            {
                Id = r.Id,
                TicketId = r.TicketId,
                UserId = r.UserId,
                User = new ApplicationUser { Id = r.UserId, DisplayName = r.UserName },
                Content = r.Content,
                CreatedAt = r.CreatedAt
            }).ToList();
        }
    }
}
