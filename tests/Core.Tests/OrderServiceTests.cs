using Xunit;
using Moq;
using Core;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Core.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IProductRepository> _mockProductRepository; // Though not used in current OrderService methods, good to have for consistency
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _orderService = new OrderService(_mockOrderRepository.Object, _mockProductRepository.Object);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldCallAddAsyncOnRepository()
        {
            // Arrange
            var newOrder = new Order { ProductSysNo = 1, Price = 100, Quantity = 1 };
            var generatedSysNo = 123;
            _mockOrderRepository.Setup(repo => repo.AddAsync(newOrder))
                .ReturnsAsync(generatedSysNo);

            // Act
            var result = await _orderService.CreateOrderAsync(newOrder);

            // Assert
            _mockOrderRepository.Verify(repo => repo.AddAsync(newOrder), Times.Once);
            Assert.NotNull(result);
            Assert.Equal(generatedSysNo, result.SysNo); // Check if SysNo is set back by the service
        }

        [Fact]
        public async Task GetOrderByIdAsync_ShouldReturnOrder_WhenOrderExists()
        {
            // Arrange
            var expectedOrder = new Order { SysNo = 1, ProductSysNo = 1, Price = 100 };
            _mockOrderRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(expectedOrder);

            // Act
            var result = await _orderService.GetOrderByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedOrder.SysNo, result.SysNo);
            _mockOrderRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetOrderByIdAsync_ShouldReturnNull_WhenOrderDoesNotExist()
        {
            // Arrange
            _mockOrderRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _orderService.GetOrderByIdAsync(99);

            // Assert
            Assert.Null(result);
            _mockOrderRepository.Verify(repo => repo.GetByIdAsync(99), Times.Once);
        }

        [Fact]
        public async Task GetAllOrdersAsync_ShouldReturnAllOrders()
        {
            // Arrange
            var expectedOrders = new List<Order>
            {
                new Order { SysNo = 1, ProductSysNo = 1 },
                new Order { SysNo = 2, ProductSysNo = 2 }
            };
            _mockOrderRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(expectedOrders);

            // Act
            var result = await _orderService.GetAllOrdersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockOrderRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldUpdateStatus_WhenOrderExists()
        {
            // Arrange
            var orderSysNo = 1;
            short newStatus = 2;
            var existingOrder = new Order { SysNo = orderSysNo, Status = 1 };
            _mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderSysNo))
                .ReturnsAsync(existingOrder);
            _mockOrderRepository.Setup(repo => repo.UpdateAsync(It.Is<Order>(o => o.SysNo == orderSysNo && o.Status == newStatus)))
                .Returns(Task.CompletedTask);

            // Act
            await _orderService.UpdateOrderStatusAsync(orderSysNo, newStatus);

            // Assert
            _mockOrderRepository.Verify(repo => repo.GetByIdAsync(orderSysNo), Times.Once);
            _mockOrderRepository.Verify(repo => repo.UpdateAsync(It.Is<Order>(o => o.SysNo == orderSysNo && o.Status == newStatus)), Times.Once);
            Assert.Equal(newStatus, existingOrder.Status); // Verify the status was updated on the object passed to UpdateAsync
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldThrowException_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderSysNo = 99;
            short newStatus = 2;
            _mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderSysNo))
                .ReturnsAsync((Order)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _orderService.UpdateOrderStatusAsync(orderSysNo, newStatus));
            Assert.Equal($"Order with SysNo {orderSysNo} not found.", ex.Message);
            _mockOrderRepository.Verify(repo => repo.GetByIdAsync(orderSysNo), Times.Once);
            _mockOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Never);
        }
        
        [Fact]
        public async Task DeleteOrderAsync_ShouldCallDeleteAsyncOnRepository()
        {
            // Arrange
            int orderSysNoToDelete = 1;
            _mockOrderRepository.Setup(repo => repo.DeleteAsync(orderSysNoToDelete))
                .Returns(Task.CompletedTask);

            // Act
            await _orderService.DeleteOrderAsync(orderSysNoToDelete);

            // Assert
            _mockOrderRepository.Verify(repo => repo.DeleteAsync(orderSysNoToDelete), Times.Once);
        }
    }
}
