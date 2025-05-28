using Xunit;
using Moq;
using Core;
using Data;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dapper;

namespace Core.Tests
{
    public class ProductOnShopifyRepositoryTests
    {
        private readonly Mock<IDbConnectionFactory> _mockDbConnectionFactory;
        private readonly Mock<IDbConnection> _mockDbConnection;
        private readonly ProductOnShopifyRepository _repository;

        public ProductOnShopifyRepositoryTests()
        {
            _mockDbConnection = new Mock<IDbConnection>();
            _mockDbConnectionFactory = new Mock<IDbConnectionFactory>();
            _mockDbConnectionFactory.Setup(x => x.CreateConnection()).Returns(_mockDbConnection.Object);
            
            // Mocking Dapper extension methods requires a bit of setup.
            // We'll set up specific Dapper calls within each test method as needed.

            _repository = new ProductOnShopifyRepository(_mockDbConnectionFactory.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldCallExecuteAsyncWithCorrectSqlAndEntity()
        {
            // Arrange
            var entity = new ProductOnShopify { ProductSysNo = 1, ShopifyProductId = "shopify-123", Version = "1.0" };
            _mockDbConnection.Setup(db => db.ExecuteAsync(
                It.Is<string>(s => s.Contains("INSERT INTO ProductOnShopify")),
                entity, null, null, null))
                .ReturnsAsync(1);

            // Act
            await _repository.AddAsync(entity);

            // Assert
            _mockDbConnection.Verify(db => db.ExecuteAsync(
                It.Is<string>(s => s.Contains("INSERT INTO ProductOnShopify") && s.Contains("VALUES (@ProductSysNo, @ShopifyProductId, @Version, @LastSyncedAt)")),
                entity, null, null, CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldCallQueryFirstOrDefaultAsyncWithCorrectSqlAndParams()
        {
            // Arrange
            var sysNo = 1;
            var expectedEntity = new ProductOnShopify { SysNo = sysNo, ShopifyProductId = "test" };
            _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<ProductOnShopify>(
                It.Is<string>(s => s.Contains("SELECT * FROM ProductOnShopify WHERE SysNo = @SysNo")),
                It.Is<object>(o => o.GetType().GetProperty("SysNo").GetValue(o).Equals(sysNo)), 
                null, null, CommandType.Text))
                .ReturnsAsync(expectedEntity);
            
            // Act
            var result = await _repository.GetByIdAsync(sysNo);

            // Assert
            Assert.Equal(expectedEntity, result);
            _mockDbConnection.Verify(db => db.QueryFirstOrDefaultAsync<ProductOnShopify>(
                It.Is<string>(s => s.Trim() == "SELECT * FROM ProductOnShopify WHERE SysNo = @SysNo"),
                It.Is<object>(o => o.GetType().GetProperty("SysNo").GetValue(o).Equals(sysNo)),
                null, null, CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task GetByProductSysNoAsync_ShouldCallQueryFirstOrDefaultAsyncWithCorrectSqlAndParams()
        {
            // Arrange
            var productSysNo = 10;
            var expectedEntity = new ProductOnShopify { ProductSysNo = productSysNo, ShopifyProductId = "test" };
             _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<ProductOnShopify>(
                It.Is<string>(s => s.Contains("SELECT * FROM ProductOnShopify WHERE ProductSysNo = @ProductSysNo")),
                It.Is<object>(o => o.GetType().GetProperty("ProductSysNo").GetValue(o).Equals(productSysNo)), 
                null, null, CommandType.Text))
                .ReturnsAsync(expectedEntity);

            // Act
            var result = await _repository.GetByProductSysNoAsync(productSysNo);

            // Assert
            Assert.Equal(expectedEntity, result);
            _mockDbConnection.Verify(db => db.QueryFirstOrDefaultAsync<ProductOnShopify>(
                It.Is<string>(s => s.Trim() == "SELECT * FROM ProductOnShopify WHERE ProductSysNo = @ProductSysNo"),
                 It.Is<object>(o => o.GetType().GetProperty("ProductSysNo").GetValue(o).Equals(productSysNo)),
                null, null, CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task GetByShopifyProductIdAsync_ShouldCallQueryFirstOrDefaultAsyncWithCorrectSqlAndParams()
        {
            // Arrange
            var shopifyProductId = "shopify-xyz";
            var expectedEntity = new ProductOnShopify { ShopifyProductId = shopifyProductId };
            _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<ProductOnShopify>(
                It.Is<string>(s => s.Contains("SELECT * FROM ProductOnShopify WHERE ShopifyProductId = @ShopifyProductId")),
                It.Is<object>(o => o.GetType().GetProperty("ShopifyProductId").GetValue(o).Equals(shopifyProductId)), 
                null, null, CommandType.Text))
                .ReturnsAsync(expectedEntity);

            // Act
            var result = await _repository.GetByShopifyProductIdAsync(shopifyProductId);

            // Assert
            Assert.Equal(expectedEntity, result);
            _mockDbConnection.Verify(db => db.QueryFirstOrDefaultAsync<ProductOnShopify>(
                It.Is<string>(s => s.Trim() == "SELECT * FROM ProductOnShopify WHERE ShopifyProductId = @ShopifyProductId"),
                It.Is<object>(o => o.GetType().GetProperty("ShopifyProductId").GetValue(o).Equals(shopifyProductId)),
                null, null, CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldCallQueryAsyncWithCorrectSql()
        {
            // Arrange
            var expectedEntities = new List<ProductOnShopify> { new ProductOnShopify(), new ProductOnShopify() };
            _mockDbConnection.Setup(db => db.QueryAsync<ProductOnShopify>(
                It.Is<string>(s => s.Contains("SELECT * FROM ProductOnShopify")),
                null, null, null, CommandType.Text))
                .ReturnsAsync(expectedEntities);
            
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(expectedEntities, result);
            _mockDbConnection.Verify(db => db.QueryAsync<ProductOnShopify>(
                "SELECT * FROM ProductOnShopify", null, null, null, CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallExecuteAsyncWithCorrectSqlAndEntity()
        {
            // Arrange
            var entity = new ProductOnShopify { SysNo = 1, ProductSysNo = 1, ShopifyProductId = "shopify-123", Version = "1.1" };
            _mockDbConnection.Setup(db => db.ExecuteAsync(
                It.Is<string>(s => s.Contains("UPDATE ProductOnShopify")),
                entity, null, null, null))
                .ReturnsAsync(1);

            // Act
            await _repository.UpdateAsync(entity);

            // Assert
            _mockDbConnection.Verify(db => db.ExecuteAsync(
                It.Is<string>(s => s.Contains("UPDATE ProductOnShopify") && s.Contains("SET ProductSysNo = @ProductSysNo") && s.Contains("WHERE SysNo = @SysNo")),
                entity, null, null, CommandType.Text), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallExecuteAsyncWithCorrectSqlAndParams()
        {
            // Arrange
            var sysNo = 1;
             _mockDbConnection.Setup(db => db.ExecuteAsync(
                It.Is<string>(s => s.Contains("DELETE FROM ProductOnShopify WHERE SysNo = @SysNo")),
                It.Is<object>(o => o.GetType().GetProperty("SysNo").GetValue(o).Equals(sysNo)), 
                null, null, CommandType.Text))
                .ReturnsAsync(1);

            // Act
            await _repository.DeleteAsync(sysNo);

            // Assert
            _mockDbConnection.Verify(db => db.ExecuteAsync(
                "DELETE FROM ProductOnShopify WHERE SysNo = @SysNo",
                It.Is<object>(o => o.GetType().GetProperty("SysNo").GetValue(o).Equals(sysNo)),
                null, null, CommandType.Text), Times.Once);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenDbConnectionFactoryIsNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ProductOnShopifyRepository(null));
            Assert.Equal("dbConnectionFactory", exception.ParamName);
        }
    }
}
