using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core
{
    public interface IProductOnShopifyRepository
    {
        Task<ProductOnShopify> GetByIdAsync(int sysNo);
        Task<ProductOnShopify> GetByProductSysNoAsync(int productSysNo);
        Task<ProductOnShopify> GetByShopifyProductIdAsync(string shopifyProductId);
        Task<IEnumerable<ProductOnShopify>> GetAllAsync();
        Task AddAsync(ProductOnShopify entity);
        Task UpdateAsync(ProductOnShopify entity);
        Task DeleteAsync(int sysNo);
    }
}
