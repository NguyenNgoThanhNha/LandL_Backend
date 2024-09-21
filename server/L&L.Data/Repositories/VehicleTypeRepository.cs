using L_L.Data.Base;
using L_L.Data.Entities;

namespace L_L.Data.Repositories
{
    public class VehicleTypeRepository : GenericRepository<VehicleType, int>
    {
        public VehicleTypeRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
