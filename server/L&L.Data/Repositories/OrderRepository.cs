using L_L.Data.Base;
using L_L.Data.Entities;

namespace L_L.Data.Repositories
{
    public class OrderRepository : GenericRepository<Order, int>
    {
        public OrderRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
