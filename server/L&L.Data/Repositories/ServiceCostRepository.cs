using L_L.Data.Base;
using L_L.Data.Entities;

namespace L_L.Data.Repositories;

public class ServiceCostRepository : GenericRepository<ServiceCost, int>
{
    public ServiceCostRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}