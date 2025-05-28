using System;
using System.Threading.Tasks;

namespace Core
{
    public interface IShopifySyncService
    {
        Task SynchronizeProductVersionAsync(string shopifyProductId, string newVersion, DateTime syncTimestamp);
        Task DelistProductAsync(string shopifyProductId);
        Task UpdateProductAsync(string shopifyProductId, Product productDetails);
        Task<string> CreateProductAsync(Product productDetails);
    }
}
