using Core; // For ExampleEntity
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces // Or just namespace Core
{
    public interface IExampleRepository
    {
        Task<ExampleEntity> GetByIdAsync(int id);
        Task<IEnumerable<ExampleEntity>> GetAllAsync();
        // Add other methods like AddAsync, UpdateAsync, DeleteAsync as needed later
    }
}
