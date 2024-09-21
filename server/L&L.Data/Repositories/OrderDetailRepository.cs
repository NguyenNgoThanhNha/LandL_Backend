using L_L.Data.Base;
using L_L.Data.Entities;

namespace L_L.Data.Repositories
{
    public class OrderDetailRepository : GenericRepository<OrderDetails, int>
    {
        public OrderDetailRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
