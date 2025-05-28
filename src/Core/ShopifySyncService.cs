using System;
using System.Threading.Tasks;
using Core; // Already here, but good to note for Product type
using Microsoft.Extensions.Options;
using ShopifySharp;
using System.Linq;
using System.Collections.Generic;

namespace Core
{
    public class ShopifySyncService : IShopifySyncService
    {
        private readonly IProductOnShopifyRepository _productOnShopifyRepository;
        private readonly ShopifyApiSettings _shopifyApiSettings;

        public ShopifySyncService(IProductOnShopifyRepository productOnShopifyRepository, IOptions<ShopifyApiSettings> shopifyApiSettings)
        {
            _productOnShopifyRepository = productOnShopifyRepository ?? throw new ArgumentNullException(nameof(productOnShopifyRepository));
            _shopifyApiSettings = shopifyApiSettings?.Value ?? throw new ArgumentNullException(nameof(shopifyApiSettings));

            if (string.IsNullOrEmpty(_shopifyApiSettings.ShopifyStoreUrl) ||
                string.IsNullOrEmpty(_shopifyApiSettings.ShopifyApiPassword)) // API Password acts as Access Token
            {
                throw new ArgumentException("Shopify API settings (StoreUrl, ApiPassword) are not configured properly.");
            }
        }

        public async Task SynchronizeProductVersionAsync(string shopifyProductId, string newVersion, DateTime syncTimestamp)
        {
            if (string.IsNullOrEmpty(shopifyProductId))
            {
                // Or throw new ArgumentException("Shopify Product ID cannot be null or empty.", nameof(shopifyProductId));
                // For now, just return if the ID is invalid, as per simplified error handling.
                Console.WriteLine("Shopify Product ID cannot be null or empty."); // Placeholder for logging
                return;
            }

            var productLink = await _productOnShopifyRepository.GetByShopifyProductIdAsync(shopifyProductId);

            if (productLink != null)
            {
                productLink.Version = newVersion;
                productLink.LastSyncedAt = syncTimestamp;
                await _productOnShopifyRepository.UpdateAsync(productLink);
                // Console.WriteLine($"Product with Shopify ID {shopifyProductId} version synchronized to {newVersion}."); // Placeholder for logging
            }
            else
            {
                // Log a warning: Shopify product not found in local system for synchronization.
                // In a real scenario, this might involve creating a new link or other business logic.
                Console.WriteLine($"Shopify product with ID {shopifyProductId} not found in local system for synchronization."); // Placeholder for logging
            }
        }

        public async Task DelistProductAsync(string shopifyProductId)
        {
            if (string.IsNullOrEmpty(shopifyProductId))
            {
                Console.WriteLine("Shopify Product ID cannot be null or empty for delisting."); // Placeholder for logging
                // Or throw new ArgumentException("Shopify Product ID is required.", nameof(shopifyProductId));
                return;
            }

            try
            {
                var productService = new ShopifySharp.ProductService(_shopifyApiSettings.ShopifyStoreUrl, _shopifyApiSettings.ShopifyApiPassword);
                if (!long.TryParse(shopifyProductId, out long longShopifyProductId))
                {
                    Console.WriteLine($"Invalid Shopify Product ID format: {shopifyProductId}"); // Placeholder for logging
                    // Or throw new ArgumentException("Invalid Shopify Product ID format.", nameof(shopifyProductId));
                    return;
                }
                
                await productService.UnpublishAsync(longShopifyProductId);
                Console.WriteLine($"Product {shopifyProductId} unpublished successfully from Shopify."); // Placeholder for logging
            }
            catch (ShopifyException ex)
            {
                Console.WriteLine($"Shopify API error during delisting product {shopifyProductId}: {ex.Message}"); // Placeholder for logging
                // Potentially rethrow or handle more gracefully
                // throw; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred during delisting product {shopifyProductId}: {ex.Message}"); // Placeholder for logging
                // throw;
            }
        }

        public async Task UpdateProductAsync(string shopifyProductId, Product productDetails)
        {
            if (string.IsNullOrEmpty(shopifyProductId))
            {
                Console.WriteLine("Shopify Product ID cannot be null or empty for updating."); // Placeholder for logging
                return;
            }
            if (productDetails == null)
            {
                Console.WriteLine("Product details cannot be null for updating."); // Placeholder for logging
                return;
            }

            try
            {
                var productService = new ShopifySharp.ProductService(_shopifyApiSettings.ShopifyStoreUrl, _shopifyApiSettings.ShopifyApiPassword);
                if (!long.TryParse(shopifyProductId, out long longShopifyProductId))
                {
                    Console.WriteLine($"Invalid Shopify Product ID format: {shopifyProductId}"); // Placeholder for logging
                    return;
                }

                var shopifyProductToUpdate = new ShopifySharp.Product
                {
                    Id = longShopifyProductId,
                    Title = productDetails.ProductName,
                    BodyHtml = productDetails.Description ?? "Updated product description.", // Assuming Product has Description
                    // Vendor = productDetails.Vendor, // If available in Core.Product
                    // ProductType = productDetails.ProductType, // If available in Core.Product
                };

                // Note: Price and Inventory are typically managed at the ProductVariant level.
                // Updating them here would require fetching variants, choosing one, and updating it.
                // For simplicity, this example focuses on product-level fields.
                // A more complete implementation would involve:
                // var variantService = new ShopifySharp.ProductVariantService(_shopifyApiSettings.ShopifyStoreUrl, _shopifyApiSettings.ShopifyApiPassword);
                // var variants = await variantService.ListAsync(longShopifyProductId);
                // if (variants.Any()) {
                //   var firstVariant = variants.Items.First();
                //   firstVariant.Price = productDetails.Price;
                //   // Shopify's InventoryQuantity is managed through InventoryLevelService or by setting InventoryManagement on variant.
                //   // firstVariant.InventoryQuantity = productDetails.Inventory; // This might not work directly depending on Shopify settings
                //   await variantService.UpdateAsync(firstVariant.Id.Value, firstVariant);
                // }
                // Console.WriteLine("Price/Inventory update requires variant management - acknowledge complexity.");

                var updatedShopifyProduct = await productService.UpdateAsync(longShopifyProductId, shopifyProductToUpdate);
                Console.WriteLine($"Product {updatedShopifyProduct.Id} ({updatedShopifyProduct.Title}) updated successfully on Shopify.");

                // Optionally, update the local ProductOnShopify link's LastSyncedAt or Version if needed
                var localLink = await _productOnShopifyRepository.GetByShopifyProductIdAsync(shopifyProductId);
                if (localLink != null)
                {
                    localLink.LastSyncedAt = DateTime.UtcNow;
                    // localLink.Version = updatedShopifyProduct.UpdatedAt?.ToString("o"); // Or some other versioning scheme
                    await _productOnShopifyRepository.UpdateAsync(localLink);
                }
            }
            catch (ShopifyException ex)
            {
                Console.WriteLine($"Shopify API error during updating product {shopifyProductId}: {ex.Message}"); // Placeholder for logging
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred during updating product {shopifyProductId}: {ex.Message}"); // Placeholder for logging
            }
        }

        public async Task<string> CreateProductAsync(Product productDetails)
        {
            if (productDetails == null)
            {
                Console.WriteLine("Product details cannot be null for creation."); // Placeholder for logging
                // Or throw new ArgumentNullException(nameof(productDetails));
                return null;
            }

            try
            {
                var productService = new ShopifySharp.ProductService(_shopifyApiSettings.ShopifyStoreUrl, _shopifyApiSettings.ShopifyApiPassword);

                var shopifyProductToCreate = new ShopifySharp.Product
                {
                    Title = productDetails.ProductName,
                    BodyHtml = productDetails.Description ?? "Product created via API.", // Assuming Core.Product has Description
                    Vendor = "YourVendorName", // Placeholder or from productDetails if available
                    ProductType = "General",   // Placeholder or from productDetails if available
                    Variants = new List<ShopifySharp.ProductVariant>
                    {
                        new ShopifySharp.ProductVariant
                        {
                            Price = productDetails.Price,
                            // Sku = productDetails.SKU, // If available in Core.Product
                            // InventoryManagement = "shopify", // Let Shopify track inventory
                            // InventoryQuantity = productDetails.Inventory, // Set initial inventory
                        }
                    },
                    // PublishedAt = DateTime.UtcNow // Publish immediately. Omit or set to null for draft.
                };
                
                // For inventory, it's better to set it after creation if using locations
                // or ensure InventoryManagement is properly set.
                // If productDetails.Inventory > 0, set InventoryManagement to "shopify" for the variant.
                if (productDetails.Inventory > 0 && shopifyProductToCreate.Variants.First() != null) {
                    shopifyProductToCreate.Variants.First().InventoryManagement = "shopify";
                    // The actual inventory quantity is set via InventoryLevelService.SetAsync after product/variant creation.
                    // shopifyProductToCreate.Variants.First().InventoryQuantity = productDetails.Inventory; // This is more of a desired state for some APIs
                }


                var createdShopifyProduct = await productService.CreateAsync(shopifyProductToCreate, new ProductCreateOptions { Published = false }); // Create as draft first

                if (createdShopifyProduct != null && createdShopifyProduct.Id.HasValue)
                {
                    Console.WriteLine($"Product {createdShopifyProduct.Id} ({createdShopifyProduct.Title}) created successfully on Shopify.");

                    // If using locations and want to set inventory:
                    if (productDetails.Inventory > 0 && createdShopifyProduct.Variants.Any() && createdShopifyProduct.Variants.First().InventoryItemId.HasValue)
                    {
                        var inventoryItemService = new InventoryItemService(_shopifyApiSettings.ShopifyStoreUrl, _shopifyApiSettings.ShopifyApiPassword);
                        var inventoryLevelService = new InventoryLevelService(_shopifyApiSettings.ShopifyStoreUrl, _shopifyApiSettings.ShopifyApiPassword);
                        var locationService = new LocationService(_shopifyApiSettings.ShopifyStoreUrl, _shopifyApiSettings.ShopifyApiPassword);
                        
                        var locations = await locationService.ListAsync();
                        var firstLocation = locations.Items.FirstOrDefault();

                        if (firstLocation != null)
                        {
                            await inventoryLevelService.SetAsync(new InventoryLevel
                            {
                                Available = productDetails.Inventory,
                                LocationId = firstLocation.Id.Value,
                                InventoryItemId = createdShopifyProduct.Variants.First().InventoryItemId.Value
                            });
                             Console.WriteLine($"Inventory set for product {createdShopifyProduct.Id} at location {firstLocation.Id}.");
                        }
                        else {
                             Console.WriteLine($"Could not set inventory for product {createdShopifyProduct.Id}: No locations found.");
                        }
                    }
                    
                    // Now publish the product
                    createdShopifyProduct.PublishedAt = DateTime.UtcNow;
                    await productService.UpdateAsync(createdShopifyProduct.Id.Value, new ShopifySharp.Product { Id = createdShopifyProduct.Id.Value, PublishedAt = DateTime.UtcNow });
                    Console.WriteLine($"Product {createdShopifyProduct.Id} published on Shopify.");


                    // Link to local DB
                    var newLink = new ProductOnShopify
                    {
                        ProductSysNo = productDetails.SysNo, // Assuming Core.Product has SysNo
                        ShopifyProductId = createdShopifyProduct.Id.Value.ToString(),
                        Version = createdShopifyProduct.UpdatedAt?.ToString("o") ?? DateTime.UtcNow.ToString("o"), // Use Shopify's UpdatedAt as version
                        LastSyncedAt = DateTime.UtcNow
                    };
                    await _productOnShopifyRepository.AddAsync(newLink);
                    Console.WriteLine($"Product {createdShopifyProduct.Id} linked to local ProductSysNo {productDetails.SysNo}.");

                    return createdShopifyProduct.Id.Value.ToString();
                }
                else
                {
                    Console.WriteLine("Shopify product creation failed or did not return an ID.");
                    return null;
                }
            }
            catch (ShopifyException ex)
            {
                Console.WriteLine($"Shopify API error during creating product: {ex.Message}. Errors: {string.Join(", ", ex.Errors.Select(e => $"{e.Key}: {string.Join(";", e.Value)}"))}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred during creating product: {ex.Message}");
                return null;
            }
        }
    }
}
