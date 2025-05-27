using Core;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Data
{
    public class ProductColorRepository : IProductColorRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public ProductColorRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IEnumerable<Product_Color>> GetByProductSysNoAsync(int productSysNo)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                // Ensure the table and column names match your database schema
                return await connection.QueryAsync<Product_Color>("SELECT * FROM Product_Colors WHERE ProductSysNo = @ProductSysNo", new { ProductSysNo = productSysNo });
            }
        }

        public async Task AddAsync(Product_Color productColor)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                // Ensure the table and column names match your database schema
                var sql = "INSERT INTO Product_Colors (ProductSysNo, Color) VALUES (@ProductSysNo, @Color)";
                await connection.ExecuteAsync(sql, productColor);
            }
        }

        public async Task DeleteAsync(int productSysNo, string color)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                // Ensure the table and column names match your database schema
                await connection.ExecuteAsync("DELETE FROM Product_Colors WHERE ProductSysNo = @ProductSysNo AND Color = @Color", new { ProductSysNo = productSysNo, Color = color });
            }
        }
        
        // Example for bulk adding colors, assuming Product_Color has a composite primary key (ProductSysNo, Color)
        // public async Task AddMultipleAsync(IEnumerable<Product_Color> productColors)
        // {
        //     using (var connection = _dbConnectionFactory.CreateConnection())
        //     {
        //         // Ensure the table and column names match your database schema
        //         var sql = "INSERT INTO Product_Colors (ProductSysNo, Color) VALUES (@ProductSysNo, @Color)";
        //         await connection.ExecuteAsync(sql, productColors);
        //     }
        // }

        // Example for deleting all colors for a product
        // public async Task DeleteByProductSysNoAsync(int productSysNo)
        // {
        //     using (var connection = _dbConnectionFactory.CreateConnection())
        //     {
        //         await connection.ExecuteAsync("DELETE FROM Product_Colors WHERE ProductSysNo = @ProductSysNo", new { ProductSysNo = productSysNo });
        //     }
        // }
    }
}
