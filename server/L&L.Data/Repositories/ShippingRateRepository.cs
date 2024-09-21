using L_L.Data.Base;
using L_L.Data.Entities;

namespace L_L.Data.Repositories
{
    public class ShippingRateRepository : GenericRepository<ShippingRate, int>
    {
        public ShippingRateRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
