using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using TicketFlow.Application.Common.Interfaces;

namespace TicketFlow.Infrastructure.Persistence
{
    /// <summary>
    /// PostgreSQL connection factory utilizing Npgsql for Dapper queries.
    /// </summary>
    public class NpgsqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;

        public NpgsqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection connection string is not configured.");
        }

        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
