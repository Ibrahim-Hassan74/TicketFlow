using System.Data;

namespace TicketFlow.Application.Common.Interfaces
{
    /// <summary>
    /// Factory for creating open or openable database connections for high-performance Dapper read queries.
    /// </summary>
    public interface ISqlConnectionFactory
    {
        /// <summary>
        /// Creates a new database connection.
        /// </summary>
        IDbConnection CreateConnection();
    }
}
