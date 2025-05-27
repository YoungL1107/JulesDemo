using Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        // DTO for updating order status
        public class UpdateOrderStatusRequest
        {
            public short Status { get; set; }
        }

        // POST /api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            if (order == null)
            {
                return BadRequest("Order data is null.");
            }
            // Ensure SysNo is not set by client, or handle appropriately
            // For this example, we assume it's a new order and SysNo should be generated
            order.SysNo = 0; 
            var createdOrder = await _orderService.CreateOrderAsync(order);
            return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.SysNo }, createdOrder);
        }

        // GET /api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        // GET /api/orders
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        // PUT /api/orders/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is null.");
            }
            try
            {
                await _orderService.UpdateOrderStatusAsync(id, request.Status);
                return Ok();
            }
            catch (Exception ex) // Replace with specific exception like OrderNotFoundException if defined
            {
                // Log the exception ex
                if (ex.Message.Contains("not found")) // Basic check, improve with custom exceptions
                {
                    return NotFound(ex.Message);
                }
                return StatusCode(500, "An error occurred while updating the order status.");
            }
        }

        // DELETE /api/orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                // First, check if the order exists to return a more informative response
                // Although DeleteOrderAsync itself might handle it, this allows a clear NotFound.
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound($"Order with ID {id} not found.");
                }
                await _orderService.DeleteOrderAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception ex
                // In a real app, you might have more specific error handling
                return StatusCode(500, $"An error occurred while deleting order {id}: {ex.Message}");
            }
        }
    }
}
