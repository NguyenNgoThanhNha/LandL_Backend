using L_L.Data.Base;
using L_L.Data.Entities;

namespace L_L.Data.Repositories
{
    public class ProductRepository : GenericRepository<Product, int>
    {
        public ProductRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
