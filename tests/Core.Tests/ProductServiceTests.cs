using Xunit;
using Moq;
using Core;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Core.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IProductColorRepository> _mockProductColorRepository;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockProductColorRepository = new Mock<IProductColorRepository>();
            _productService = new ProductService(_mockProductRepository.Object, _mockProductColorRepository.Object);
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExists()
        {
            // Arrange
            var expectedProduct = new Product { SysNo = 1, ProductName = "Test Product" };
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(expectedProduct);

            // Act
            var result = await _productService.GetProductByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProduct.SysNo, result.SysNo);
            Assert.Equal(expectedProduct.ProductName, result.ProductName);
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _productService.GetProductByIdAsync(99);

            // Assert
            Assert.Null(result);
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(99), Times.Once);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        {
            // Arrange
            var expectedProducts = new List<Product>
            {
                new Product { SysNo = 1, ProductName = "Product 1" },
                new Product { SysNo = 2, ProductName = "Product 2" }
            };
            _mockProductRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(expectedProducts);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockProductRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task AddProductAsync_ShouldCallRepositoryAddAsync()
        {
            // Arrange
            var newProduct = new Product { ProductName = "New Product", Price = 10.99m };
            _mockProductRepository.Setup(repo => repo.AddAsync(newProduct))
                .Returns(Task.CompletedTask); // Assuming AddAsync returns Task and might modify product (e.g. SysNo)

            // Act
            await _productService.AddProductAsync(newProduct);

            // Assert
            _mockProductRepository.Verify(repo => repo.AddAsync(newProduct), Times.Once);
        }
        
        [Fact]
        public async Task UpdateProductAsync_ShouldCallRepositoryUpdateAsync()
        {
            // Arrange
            var productToUpdate = new Product { SysNo = 1, ProductName = "Updated Product" };
            _mockProductRepository.Setup(repo => repo.UpdateAsync(productToUpdate))
                .Returns(Task.CompletedTask);

            // Act
            await _productService.UpdateProductAsync(productToUpdate);

            // Assert
            _mockProductRepository.Verify(repo => repo.UpdateAsync(productToUpdate), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_ShouldCallRepositoryDeleteAsync()
        {
            // Arrange
            int productSysNoToDelete = 1;
            _mockProductRepository.Setup(repo => repo.DeleteAsync(productSysNoToDelete))
                .Returns(Task.CompletedTask);

            // Act
            await _productService.DeleteProductAsync(productSysNoToDelete);

            // Assert
            _mockProductRepository.Verify(repo => repo.DeleteAsync(productSysNoToDelete), Times.Once);
        }

        // --- Product Color Tests ---

        [Fact]
        public async Task GetColorsForProductAsync_ShouldReturnColors_WhenProductHasColors()
        {
            // Arrange
            int productSysNo = 1;
            var expectedColors = new List<Product_Color>
            {
                new Product_Color { ProductSysNo = productSysNo, Color = "Red" },
                new Product_Color { ProductSysNo = productSysNo, Color = "Blue" }
            };
            _mockProductColorRepository.Setup(repo => repo.GetByProductSysNoAsync(productSysNo))
                .ReturnsAsync(expectedColors);

            // Act
            var result = await _productService.GetColorsForProductAsync(productSysNo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockProductColorRepository.Verify(repo => repo.GetByProductSysNoAsync(productSysNo), Times.Once);
        }

        [Fact]
        public async Task AddColorToProductAsync_ShouldCallRepositoryAddAsync()
        {
            // Arrange
            var newColor = new Product_Color { ProductSysNo = 1, Color = "Green" };
            _mockProductColorRepository.Setup(repo => repo.AddAsync(newColor))
                .Returns(Task.CompletedTask);

            // Act
            await _productService.AddColorToProductAsync(newColor);

            // Assert
            _mockProductColorRepository.Verify(repo => repo.AddAsync(newColor), Times.Once);
        }

        [Fact]
        public async Task RemoveColorFromProductAsync_ShouldCallRepositoryDeleteAsync()
        {
            // Arrange
            int productSysNo = 1;
            string colorToRemove = "Red";
            _mockProductColorRepository.Setup(repo => repo.DeleteAsync(productSysNo, colorToRemove))
                .Returns(Task.CompletedTask);

            // Act
            await _productService.RemoveColorFromProductAsync(productSysNo, colorToRemove);

            // Assert
            _mockProductColorRepository.Verify(repo => repo.DeleteAsync(productSysNo, colorToRemove), Times.Once);
        }
    }
}
