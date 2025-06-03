using In_Memory_Caching.DTOs;
using In_Memory_Caching.Models;

namespace In_Memory_Caching.Services
{
    public interface IProductService
    {
        public Task Add(ProductCreationDto request);
        public Task<Product> Get(Guid id);
        public Task<List<Product>> GetAll();
    }
}
