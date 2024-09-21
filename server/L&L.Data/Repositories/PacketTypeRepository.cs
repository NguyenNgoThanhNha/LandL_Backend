using L_L.Data.Base;
using L_L.Data.Entities;

namespace L_L.Data.Repositories
{
    public class PacketTypeRepository : GenericRepository<PackageType, int>
    {
        public PacketTypeRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
