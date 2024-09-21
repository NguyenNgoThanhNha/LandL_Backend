using L_L.Data.Base;
using L_L.Data.Entities;

namespace L_L.Data.Repositories
{
    public class TruckRepository : GenericRepository<Truck, int>
    {
        public TruckRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
