using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(int sysNo);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<int> AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(int sysNo);
    }
}
