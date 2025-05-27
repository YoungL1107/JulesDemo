using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository; // For future use

        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }
            // In a real-world scenario, you might validate ProductSysNo using _productRepository
            // or check stock levels before creating the order.
            // For now, we directly add the order.
            var orderId = await _orderRepository.AddAsync(order);
            order.SysNo = orderId; // Assuming AddAsync returns the new SysNo and we set it back.
            return order;
        }

        public async Task<Order> GetOrderByIdAsync(int orderSysNo)
        {
            return await _orderRepository.GetByIdAsync(orderSysNo);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllAsync();
        }

        public async Task UpdateOrderStatusAsync(int orderSysNo, short newStatus)
        {
            var order = await _orderRepository.GetByIdAsync(orderSysNo);
            if (order == null)
            {
                // Or throw a custom OrderNotFoundException
                throw new Exception($"Order with SysNo {orderSysNo} not found.");
            }
            order.Status = newStatus;
            await _orderRepository.UpdateAsync(order);
        }

        public async Task DeleteOrderAsync(int orderSysNo)
        {
            await _orderRepository.DeleteAsync(orderSysNo);
        }
    }
}
