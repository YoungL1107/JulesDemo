using Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        // --- Product Endpoints ---

        [HttpGet("{sysNo}")]
        public async Task<ActionResult<Product>> GetProduct(int sysNo)
        {
            var product = await _productService.GetProductByIdAsync(sysNo);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _productService.AddProductAsync(product);
            // Assuming AddProductAsync updates the product with SysNo if it's an identity column
            return CreatedAtAction(nameof(GetProduct), new { sysNo = product.SysNo }, product);
        }

        [HttpPut("{sysNo}")]
        public async Task<IActionResult> UpdateProduct(int sysNo, Product product)
        {
            if (sysNo != product.SysNo)
            {
                return BadRequest("Product SysNo mismatch.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                await _productService.UpdateProductAsync(product);
            }
            catch (Exception) // Replace with more specific exception handling, e.g., KeyNotFoundException
            {
                // This is a simplistic check. Ideally, UpdateProductAsync would signal if the product didn't exist.
                var existingProduct = await _productService.GetProductByIdAsync(sysNo);
                if (existingProduct == null)
                {
                    return NotFound();
                }
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{sysNo}")]
        public async Task<IActionResult> DeleteProduct(int sysNo)
        {
            var existingProduct = await _productService.GetProductByIdAsync(sysNo);
            if (existingProduct == null)
            {
                return NotFound();
            }
            await _productService.DeleteProductAsync(sysNo);
            return NoContent();
        }

        // --- Product Color Endpoints ---

        [HttpGet("{productSysNo}/colors")]
        public async Task<ActionResult<IEnumerable<Product_Color>>> GetProductColors(int productSysNo)
        {
            var product = await _productService.GetProductByIdAsync(productSysNo);
            if (product == null)
            {
                return NotFound("Product not found.");
            }
            var colors = await _productService.GetColorsForProductAsync(productSysNo);
            return Ok(colors);
        }

        [HttpPost("{productSysNo}/colors")]
        public async Task<IActionResult> AddProductColor(int productSysNo, Product_Color productColor)
        {
            if (productSysNo != productColor.ProductSysNo)
            {
                return BadRequest("ProductSysNo mismatch in route and body.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var product = await _productService.GetProductByIdAsync(productSysNo);
            if (product == null)
            {
                return NotFound("Product not found.");
            }
            await _productService.AddColorToProductAsync(productColor);
            // Consider how to identify the created color resource, e.g., a composite key
            return Ok(); // Or CreatedAtAction if you have a Get specific color endpoint
        }

        [HttpDelete("{productSysNo}/colors/{color}")]
        public async Task<IActionResult> RemoveProductColor(int productSysNo, string color)
        {
             var product = await _productService.GetProductByIdAsync(productSysNo);
            if (product == null)
            {
                return NotFound("Product not found.");
            }
            // Optional: Check if the color actually exists for the product before attempting delete
            await _productService.RemoveColorFromProductAsync(productSysNo, color);
            return NoContent();
        }
    }
}
