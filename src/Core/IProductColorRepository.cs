using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core
{
    public interface IProductColorRepository
    {
        Task<IEnumerable<Product_Color>> GetByProductSysNoAsync(int productSysNo);
        Task AddAsync(Product_Color productColor);
        Task DeleteAsync(int productSysNo, string color);
        // Depending on requirements, you might also want an UpdateAsync or methods to manage colors for a product in bulk.
    }
}
