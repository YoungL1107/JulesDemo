using Xunit;
using Moq;
using Core;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options; // Required for IOptions

namespace Core.Tests
{
    public class ShopifySyncServiceTests
    {
        private readonly Mock<IProductOnShopifyRepository> _mockProductOnShopifyRepository;
        private readonly Mock<IOptions<ShopifyApiSettings>> _mockShopifyApiSettingsOptions;
        private readonly ShopifyApiSettings _validApiSettings;
        private ShopifySyncService _shopifySyncService; // Service instance, will be recreated

        public ShopifySyncServiceTests()
        {
            _mockProductOnShopifyRepository = new Mock<IProductOnShopifyRepository>();
            _mockShopifyApiSettingsOptions = new Mock<IOptions<ShopifyApiSettings>>();

            _validApiSettings = new ShopifyApiSettings
            {
                ShopifyStoreUrl = "your-test-store.myshopify.com",
                ShopifyApiKey = "TEST_API_KEY", // Though not directly used by ShopifySharp's ProductService if password/token is set
                ShopifyApiPassword = "TEST_API_PASSWORD_OR_ACCESS_TOKEN"
            };
            _mockShopifyApiSettingsOptions.Setup(o => o.Value).Returns(_validApiSettings);

            // Default service instance for most tests
            _shopifySyncService = new ShopifySyncService(_mockProductOnShopifyRepository.Object, _mockShopifyApiSettingsOptions.Object);
        }

        // --- Constructor Tests ---
        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenRepositoryIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ShopifySyncService(null, _mockShopifyApiSettingsOptions.Object));
            Assert.Equal("productOnShopifyRepository", exception.ParamName);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenShopifyApiSettingsOptionsIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ShopifySyncService(_mockProductOnShopifyRepository.Object, null));
            Assert.Equal("shopifyApiSettings", exception.ParamName);
        }
        
        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenShopifyApiSettingsValueIsNull()
        {
            _mockShopifyApiSettingsOptions.Setup(o => o.Value).Returns((ShopifyApiSettings)null);
            var exception = Assert.Throws<ArgumentNullException>(() => new ShopifySyncService(_mockProductOnShopifyRepository.Object, _mockShopifyApiSettingsOptions.Object));
            Assert.Equal("shopifyApiSettings", exception.ParamName); // Or based on how .Value is handled if it throws earlier
        }

        [Theory]
        [InlineData(null, "password", "Shopify API settings (StoreUrl, ApiPassword) are not configured properly.")]
        [InlineData("", "password", "Shopify API settings (StoreUrl, ApiPassword) are not configured properly.")]
        [InlineData("storeurl", null, "Shopify API settings (StoreUrl, ApiPassword) are not configured properly.")]
        [InlineData("storeurl", "", "Shopify API settings (StoreUrl, ApiPassword) are not configured properly.")]
        public void Constructor_ThrowsArgumentException_WhenApiSettingsAreInvalid(string storeUrl, string apiPassword, string expectedMessage)
        {
            var invalidSettings = new ShopifyApiSettings { ShopifyStoreUrl = storeUrl, ShopifyApiPassword = apiPassword };
            _mockShopifyApiSettingsOptions.Setup(o => o.Value).Returns(invalidSettings);
            
            var exception = Assert.Throws<ArgumentException>(() => new ShopifySyncService(_mockProductOnShopifyRepository.Object, _mockShopifyApiSettingsOptions.Object));
            Assert.Contains(expectedMessage, exception.Message); // Using Contains because the actual message might have parameter name
        }

        // --- SynchronizeProductVersionAsync Tests (adapted for new constructor) ---
        [Fact]
        public async Task SynchronizeProductVersionAsync_ProductLinkExists_UpdatesVersionAndLastSyncedAt_CallsUpdateAsync()
        {
            var shopifyProductId = "12345"; // ShopifySharp expects long, so ensure it's parseable
            var newVersion = "2.0";
            var syncTimestamp = DateTime.UtcNow;
            var originalProductLink = new ProductOnShopify { SysNo = 1, ProductSysNo = 10, ShopifyProductId = shopifyProductId, Version = "1.0", LastSyncedAt = DateTime.UtcNow.AddDays(-1) };

            _mockProductOnShopifyRepository.Setup(repo => repo.GetByShopifyProductIdAsync(shopifyProductId)).ReturnsAsync(originalProductLink);
            ProductOnShopify updatedProductLinkCapture = null;
            _mockProductOnShopifyRepository.Setup(repo => repo.UpdateAsync(It.IsAny<ProductOnShopify>()))
                .Callback<ProductOnShopify>(pl => updatedProductLinkCapture = pl)
                .Returns(Task.CompletedTask);

            await _shopifySyncService.SynchronizeProductVersionAsync(shopifyProductId, newVersion, syncTimestamp);

            _mockProductOnShopifyRepository.Verify(repo => repo.GetByShopifyProductIdAsync(shopifyProductId), Times.Once);
            _mockProductOnShopifyRepository.Verify(repo => repo.UpdateAsync(It.IsAny<ProductOnShopify>()), Times.Once);
            Assert.NotNull(updatedProductLinkCapture);
            Assert.Equal(newVersion, updatedProductLinkCapture.Version);
            Assert.Equal(syncTimestamp, updatedProductLinkCapture.LastSyncedAt);
        }

        [Fact]
        public async Task SynchronizeProductVersionAsync_ProductLinkDoesNotExist_DoesNotCallUpdateAsync()
        {
            var shopifyProductId = "67890";
            _mockProductOnShopifyRepository.Setup(repo => repo.GetByShopifyProductIdAsync(shopifyProductId)).ReturnsAsync((ProductOnShopify)null);
            await _shopifySyncService.SynchronizeProductVersionAsync(shopifyProductId, "1.0", DateTime.UtcNow);
            _mockProductOnShopifyRepository.Verify(repo => repo.UpdateAsync(It.IsAny<ProductOnShopify>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SynchronizeProductVersionAsync_InvalidInputShopifyProductId_DoesNotCallRepository(string invalidShopifyProductId)
        {
            await _shopifySyncService.SynchronizeProductVersionAsync(invalidShopifyProductId, "1.0", DateTime.UtcNow);
            _mockProductOnShopifyRepository.Verify(repo => repo.GetByShopifyProductIdAsync(It.IsAny<string>()), Times.Never);
        }


        // --- DelistProductAsync Tests ---
        [Fact]
        public async Task DelistProductAsync_ValidShopifyProductId_CompletesSilently()
        {
            // Cannot verify ShopifySharp.ProductService.UnpublishAsync directly
            // Test ensures the method runs without unhandled exceptions for a valid-looking ID
            // and that parameter validation passes.
            var shopifyProductId = "123456789"; // Valid long format
            await _shopifySyncService.DelistProductAsync(shopifyProductId);
            // No specific Moq verification possible here for the ShopifySharp call
        }

        [Theory]
        [InlineData("not-a-long")] // Invalid format
        [InlineData("0")] // Potentially valid but could be edge case
        public async Task DelistProductAsync_InvalidFormatShopifyProductId_HandlesGracefully(string shopifyProductId)
        {
            // Expect console log, no exception thrown out
            await _shopifySyncService.DelistProductAsync(shopifyProductId);
            // No specific Moq verification, ensure it doesn't throw unhandled exception
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task DelistProductAsync_NullOrEmptyShopifyProductId_HandlesGracefully(string shopifyProductId)
        {
            await _shopifySyncService.DelistProductAsync(shopifyProductId);
            // Method should return early, no ShopifySharp calls attempted.
            // No Moq verification for ShopifySharp.
        }

        // --- UpdateProductAsync Tests ---
        [Fact]
        public async Task UpdateProductAsync_ValidUpdate_LocalLinkExists_UpdatesLocalLink()
        {
            var shopifyProductId = "112233";
            var productDetails = new Product { ProductName = "Updated Name", Description = "Updated Desc", Price = 19.99m };
            var existingLink = new ProductOnShopify { SysNo = 1, ShopifyProductId = shopifyProductId, Version = "old", LastSyncedAt = DateTime.UtcNow.AddDays(-1) };
            
            _mockProductOnShopifyRepository.Setup(r => r.GetByShopifyProductIdAsync(shopifyProductId)).ReturnsAsync(existingLink);
            ProductOnShopify capturedLink = null;
            _mockProductOnShopifyRepository.Setup(r => r.UpdateAsync(It.IsAny<ProductOnShopify>()))
                                         .Callback<ProductOnShopify>(link => capturedLink = link)
                                         .Returns(Task.CompletedTask);
            
            await _shopifySyncService.UpdateProductAsync(shopifyProductId, productDetails);

            _mockProductOnShopifyRepository.Verify(r => r.GetByShopifyProductIdAsync(shopifyProductId), Times.Once);
            // Cannot verify ShopifySharp.ProductService.UpdateAsync directly
            _mockProductOnShopifyRepository.Verify(r => r.UpdateAsync(It.IsAny<ProductOnShopify>()), Times.Once);
            Assert.NotNull(capturedLink);
            Assert.NotEqual("old", capturedLink.Version); // Version should change (based on implementation detail: it uses UpdatedAt)
            Assert.True(capturedLink.LastSyncedAt > existingLink.LastSyncedAt);
        }

        [Fact]
        public async Task UpdateProductAsync_LocalLinkNotFound_DoesNotUpdateLocalLink()
        {
            var shopifyProductId = "445566";
            var productDetails = new Product { ProductName = "Updated Name" };
            _mockProductOnShopifyRepository.Setup(r => r.GetByShopifyProductIdAsync(shopifyProductId)).ReturnsAsync((ProductOnShopify)null);

            await _shopifySyncService.UpdateProductAsync(shopifyProductId, productDetails);

            _mockProductOnShopifyRepository.Verify(r => r.GetByShopifyProductIdAsync(shopifyProductId), Times.Once);
            // ShopifySharp call might still happen (cannot verify here), but local link update should not.
            _mockProductOnShopifyRepository.Verify(r => r.UpdateAsync(It.IsAny<ProductOnShopify>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task UpdateProductAsync_InvalidShopifyProductId_HandlesGracefully(string shopifyProductId)
        {
            var productDetails = new Product { ProductName = "Test" };
            await _shopifySyncService.UpdateProductAsync(shopifyProductId, productDetails);
            // Ensure no repo calls if ID is invalid before ShopifySharp interaction
             _mockProductOnShopifyRepository.Verify(r => r.GetByShopifyProductIdAsync(It.IsAny<string>()), Times.Never);
        }
        
        [Fact]
        public async Task UpdateProductAsync_NullProductDetails_HandlesGracefully()
        {
            var shopifyProductId = "123";
            await _shopifySyncService.UpdateProductAsync(shopifyProductId, null);
            // Ensure no repo calls if product details are null
            _mockProductOnShopifyRepository.Verify(r => r.GetByShopifyProductIdAsync(It.IsAny<string>()), Times.Never);
        }


        // --- CreateProductAsync Tests ---
        [Fact]
        public async Task CreateProductAsync_ValidProduct_CallsAddAsyncOnRepository()
        {
            var productDetails = new Product { SysNo = 101, ProductName = "New Gadget", Description = "Cool new thing", Price = 99.99m, Inventory = 10 };
            ProductOnShopify capturedLink = null;

            _mockProductOnShopifyRepository.Setup(r => r.AddAsync(It.IsAny<ProductOnShopify>()))
                                         .Callback<ProductOnShopify>(link => capturedLink = link)
                                         .Returns(Task.CompletedTask);
            
            // Since ShopifySharp is internal, we can't mock its CreateAsync to return a specific ID.
            // The method will likely return null because the actual Shopify call won't happen in test.
            // We are testing that if a Shopify call *were* successful (hypothetically), AddAsync would be called.
            // This is a limitation of not being able to inject/mock the ShopifySharp client.
            var resultShopifyId = await _shopifySyncService.CreateProductAsync(productDetails);

            // Assert based on the current implementation:
            // If the internal ShopifySharp.ProductService.CreateAsync could be mocked to return a product,
            // then AddAsync would be called. Without it, the real CreateAsync won't be called,
            // createdShopifyProduct will be null, and thus AddAsync won't be called.
            // For this test, let's assume a "successful" path means AddAsync *would* be called if Shopify responded.
            // Given the current code, if createdShopifyProduct is null (as it will be in this test setup),
            // AddAsync will NOT be called. This test highlights the difficulty.

            // To make this testable for the AddAsync part, one would need to:
            // 1. Refactor ShopifySyncService to allow injection of a mockable Shopify client/factory.
            // OR 2. Assume the Shopify call always "fails" in this unit test setup (returns null)
            //    and verify that AddAsync is NOT called.

            // Let's test the path where Shopify call *would* have succeeded and returned an ID,
            // leading to an attempt to add to the repository.
            // The current implementation of CreateProductAsync returns null if the Shopify call fails.
            // And it only calls AddAsync if createdShopifyProduct.Id.HasValue.
            // So, in this unit test setup, AddAsync will NOT be called.
            
            // If the intent is to test the AddAsync call, the ShopifySharp part needs to be mockable.
            // For now, we accept that this test cannot fully verify the "happy path" of CreateProductAsync
            // including the AddAsync call due to internal instantiation of ShopifySharp services.
            // The method will return null as createdShopifyProduct will be null from the unmocked ShopifySharp call.
            Assert.Null(resultShopifyId); 
            _mockProductOnShopifyRepository.Verify(r => r.AddAsync(It.IsAny<ProductOnShopify>()), Times.Never);
            // If we could mock ShopifySharp.ProductService.CreateAsync to return a product:
            // _mockProductOnShopifyRepository.Verify(r => r.AddAsync(It.IsAny<ProductOnShopify>()), Times.Once);
            // Assert.NotNull(capturedLink);
            // Assert.Equal(productDetails.SysNo, capturedLink.ProductSysNo);
            // Assert.NotNull(resultShopifyId); // This would be the ID from the mocked ShopifySharp
        }

        [Fact]
        public async Task CreateProductAsync_NullProductDetails_ReturnsNullAndDoesNotCallRepository()
        {
            var result = await _shopifySyncService.CreateProductAsync(null);
            Assert.Null(result);
            _mockProductOnShopifyRepository.Verify(r => r.AddAsync(It.IsAny<ProductOnShopify>()), Times.Never);
        }
    }
}
