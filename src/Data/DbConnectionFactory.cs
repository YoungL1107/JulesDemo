using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient; // Or Npgsql if PostgreSQL was chosen

namespace Data
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection CreateConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            // Assuming SQL Server, replace with NpgsqlConnection for PostgreSQL
            return new SqlConnection(connectionString);
        }
    }
}
