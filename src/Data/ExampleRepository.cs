using Core; // For ExampleEntity
using Core.Interfaces; // For IExampleRepository
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Data
{
    public class ExampleRepository : IExampleRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ExampleRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<ExampleEntity>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            // Ensure the table name 'ExampleEntities' matches your actual database table
            return await connection.QueryAsync<ExampleEntity>("SELECT * FROM ExampleEntities");
        }

        public async Task<ExampleEntity> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            // Ensure the table name 'ExampleEntities' and column 'Id' match your actual database
            return await connection.QuerySingleOrDefaultAsync<ExampleEntity>("SELECT * FROM ExampleEntities WHERE Id = @Id", new { Id = id });
        }
    }
}
