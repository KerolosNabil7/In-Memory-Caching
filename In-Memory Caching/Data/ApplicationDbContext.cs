using In_Memory_Caching.Models;
using Microsoft.EntityFrameworkCore;

namespace In_Memory_Caching.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
    }
}
