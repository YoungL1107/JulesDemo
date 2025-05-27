using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core
{
    public interface IProductRepository
    {
        Task<Product> GetByIdAsync(int sysNo);
        Task<IEnumerable<Product>> GetAllAsync();
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int sysNo);
    }
}
