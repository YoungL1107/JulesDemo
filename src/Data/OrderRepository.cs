using Core;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public OrderRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<int> AddAsync(Order order)
        {
            const string sql = @"
                INSERT INTO [Order] (ProductSysNo, Status, Price, Quantity, Color, ShippingAddress, PaymentStatus)
                VALUES (@ProductSysNo, @Status, @Price, @Quantity, @Color, @ShippingAddress, @PaymentStatus);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var id = await connection.QuerySingleAsync<int>(sql, order);
                return id;
            }
        }

        public async Task DeleteAsync(int sysNo)
        {
            const string sql = "DELETE FROM [Order] WHERE SysNo = @SysNo;";
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                await connection.ExecuteAsync(sql, new { SysNo = sysNo });
            }
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            const string sql = "SELECT * FROM [Order];";
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.QueryAsync<Order>(sql);
            }
        }

        public async Task<Order> GetByIdAsync(int sysNo)
        {
            const string sql = "SELECT * FROM [Order] WHERE SysNo = @SysNo;";
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<Order>(sql, new { SysNo = sysNo });
            }
        }

        public async Task UpdateAsync(Order order)
        {
            const string sql = @"
                UPDATE [Order]
                SET ProductSysNo = @ProductSysNo,
                    Status = @Status,
                    Price = @Price,
                    Quantity = @Quantity,
                    Color = @Color,
                    ShippingAddress = @ShippingAddress,
                    PaymentStatus = @PaymentStatus
                WHERE SysNo = @SysNo;";
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                await connection.ExecuteAsync(sql, order);
            }
        }
    }
}
