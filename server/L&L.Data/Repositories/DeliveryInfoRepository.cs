using L_L.Data.Base;
using L_L.Data.Entities;

namespace L_L.Data.Repositories
{
    public class DeliveryInfoRepository : GenericRepository<DeliveryInfo, int>
    {
        public DeliveryInfoRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
