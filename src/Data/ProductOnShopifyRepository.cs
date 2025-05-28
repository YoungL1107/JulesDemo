using Core;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Data
{
    public class ProductOnShopifyRepository : IProductOnShopifyRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public ProductOnShopifyRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<ProductOnShopify> GetByIdAsync(int sysNo)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<ProductOnShopify>(
                    "SELECT * FROM ProductOnShopify WHERE SysNo = @SysNo",
                    new { SysNo = sysNo });
            }
        }

        public async Task<ProductOnShopify> GetByProductSysNoAsync(int productSysNo)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<ProductOnShopify>(
                    "SELECT * FROM ProductOnShopify WHERE ProductSysNo = @ProductSysNo",
                    new { ProductSysNo = productSysNo });
            }
        }

        public async Task<ProductOnShopify> GetByShopifyProductIdAsync(string shopifyProductId)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<ProductOnShopify>(
                    "SELECT * FROM ProductOnShopify WHERE ShopifyProductId = @ShopifyProductId",
                    new { ShopifyProductId = shopifyProductId });
            }
        }

        public async Task<IEnumerable<ProductOnShopify>> GetAllAsync()
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.QueryAsync<ProductOnShopify>("SELECT * FROM ProductOnShopify");
            }
        }

        public async Task AddAsync(ProductOnShopify entity)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"
                    INSERT INTO ProductOnShopify (ProductSysNo, ShopifyProductId, Version, LastSyncedAt)
                    VALUES (@ProductSysNo, @ShopifyProductId, @Version, @LastSyncedAt)";
                await connection.ExecuteAsync(sql, entity);
            }
        }

        public async Task UpdateAsync(ProductOnShopify entity)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"
                    UPDATE ProductOnShopify
                    SET ProductSysNo = @ProductSysNo,
                        ShopifyProductId = @ShopifyProductId,
                        Version = @Version,
                        LastSyncedAt = @LastSyncedAt
                    WHERE SysNo = @SysNo";
                await connection.ExecuteAsync(sql, entity);
            }
        }

        public async Task DeleteAsync(int sysNo)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                await connection.ExecuteAsync(
                    "DELETE FROM ProductOnShopify WHERE SysNo = @SysNo",
                    new { SysNo = sysNo });
            }
        }
    }
}
