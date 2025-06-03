using In_Memory_Caching.Data;
using In_Memory_Caching.DTOs;
using In_Memory_Caching.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace In_Memory_Caching.Services
{
    public class ProductService: IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ProductService> _logger;
        public ProductService(ApplicationDbContext context, IMemoryCache cache, ILogger<ProductService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<Product>> GetAll()
        {
            var cacheKey = "products";
            _logger.LogInformation("fetching data for key: {CacheKey} from cache.", cacheKey);
            if (!_cache.TryGetValue(cacheKey, out List<Product>? products))
            {
                _logger.LogInformation("cache miss. fetching data for key: {CacheKey} from database.", cacheKey);
                products = await _context.Products.ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
                    .SetPriority(CacheItemPriority.NeverRemove)
                    .SetSize(2048);

                _logger.LogInformation("setting data for key: {CacheKey} to cache.", cacheKey);
                _cache.Set(cacheKey, products, cacheOptions);
            }
            else
            {
                _logger.LogInformation("cache hit for key: {CacheKey}.", cacheKey);
            }
            return products;
        }

        public async Task<Product> Get(Guid id)
        {
            var cacheKey = $"product:{id}";
            _logger.LogInformation("fetching data for key: {CacheKey} from cache.", cacheKey);
            if (!_cache.TryGetValue(cacheKey, out Product? product))
            {
                _logger.LogInformation("cache miss. fetching data for key: {CacheKey} from database.", cacheKey);
                product = await _context.Products.FindAsync(id);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
                    .SetPriority(CacheItemPriority.Normal);

                _logger.LogInformation("setting data for key: {CacheKey} to cache.", cacheKey);
                _cache.Set(cacheKey, product, cacheOptions);
            }
            else
            {
                _logger.LogInformation("cache hit for key: {CacheKey}.", cacheKey);
            }
            return product;
        }

        public async Task Add(ProductCreationDto request)
        {
            var product = new Product(request.Name, request.Description, request.Price);
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            // invalidate cache for products, as new product is added
            var cacheKey = "products";
            _logger.LogInformation("invalidating cache for key: {CacheKey} from cache.", cacheKey);
            _cache.Remove(cacheKey);
        }
    }
}
