using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductColorRepository _productColorRepository;

        public ProductService(IProductRepository productRepository, IProductColorRepository productColorRepository)
        {
            _productRepository = productRepository;
            _productColorRepository = productColorRepository;
        }

        // Product methods
        public Task<Product> GetProductByIdAsync(int sysNo)
        {
            return _productRepository.GetByIdAsync(sysNo);
        }

        public Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return _productRepository.GetAllAsync();
        }

        public Task AddProductAsync(Product product)
        {
            // Add any business logic/validation before adding
            return _productRepository.AddAsync(product);
        }

        public Task UpdateProductAsync(Product product)
        {
            // Add any business logic/validation before updating
            return _productRepository.UpdateAsync(product);
        }

        public Task DeleteProductAsync(int sysNo)
        {
            // Add any business logic (e.g., check if product can be deleted)
            // Potentially delete related product colors as well, or handle as per business rules
            return _productRepository.DeleteAsync(sysNo);
        }

        // Product Color methods
        public Task<IEnumerable<Product_Color>> GetColorsForProductAsync(int productSysNo)
        {
            return _productColorRepository.GetByProductSysNoAsync(productSysNo);
        }

        public Task AddColorToProductAsync(Product_Color productColor)
        {
            // Add any business logic/validation
            return _productColorRepository.AddAsync(productColor);
        }

        public Task RemoveColorFromProductAsync(int productSysNo, string color)
        {
            // Add any business logic/validation
            return _productColorRepository.DeleteAsync(productSysNo, color);
        }

        // Example of more complex logic: Update product and its colors
        // public async Task UpdateProductWithColorsAsync(Product product, IEnumerable<Product_Color> colorsToAdd, IEnumerable<Product_Color> colorsToRemove)
        // {
        //     await _productRepository.UpdateAsync(product);
        //     if (colorsToAdd != null)
        //     {
        //         foreach (var color in colorsToAdd)
        //         {
        //             // Ensure ProductSysNo is set correctly if not already
        //             color.ProductSysNo = product.SysNo;
        //             await _productColorRepository.AddAsync(color);
        //         }
        //     }
        //     if (colorsToRemove != null)
        //     {
        //         foreach (var color in colorsToRemove)
        //         {
        //             await _productColorRepository.DeleteAsync(product.SysNo, color.Color);
        //         }
        //     }
        // }
    }
}
