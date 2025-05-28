using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductColorRepository _productColorRepository;
        private readonly IProductOnShopifyRepository _productOnShopifyRepository;

        public ProductService(
            IProductRepository productRepository, 
            IProductColorRepository productColorRepository,
            IProductOnShopifyRepository productOnShopifyRepository)
        {
            _productRepository = productRepository;
            _productColorRepository = productColorRepository;
            _productOnShopifyRepository = productOnShopifyRepository;
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

        // ProductOnShopify methods
        public Task<ProductOnShopify> GetProductShopifyLinkByProductIdAsync(int productSysNo)
        {
            return _productOnShopifyRepository.GetByProductSysNoAsync(productSysNo);
        }

        public Task<ProductOnShopify> GetProductShopifyLinkByShopifyIdAsync(string shopifyProductId)
        {
            return _productOnShopifyRepository.GetByShopifyProductIdAsync(shopifyProductId);
        }

        public async Task LinkProductToShopifyAsync(ProductOnShopify productShopifyLink)
        {
            if (productShopifyLink == null)
            {
                // Consider throwing ArgumentNullException or a custom validation exception
                // For now, we'll just prevent the call if it's null.
                // Or, more robustly, throw new System.ArgumentNullException(nameof(productShopifyLink));
                return; 
            }
            // Add any business logic/validation before adding
            // e.g., check if productShopifyLink.ProductSysNo exists in Products table
            // e.g., check if shopifyProductId is a valid format or doesn't already exist for another product.
            // For this task, we assume basic validation is sufficient or handled elsewhere.
            await _productOnShopifyRepository.AddAsync(productShopifyLink);
        }

        public async Task UpdateProductShopifyLinkAsync(ProductOnShopify productShopifyLink)
        {
            if (productShopifyLink == null)
            {
                // throw new System.ArgumentNullException(nameof(productShopifyLink));
                return;
            }
            // Add any business logic/validation before updating
            await _productOnShopifyRepository.UpdateAsync(productShopifyLink);
        }

        public Task UnlinkProductFromShopifyAsync(int sysNo)
        {
            // Add any business logic (e.g., logging, cleanup)
            return _productOnShopifyRepository.DeleteAsync(sysNo);
        }
    }
}
