using L_L.Data.Base;
using L_L.Data.Entities;

namespace L_L.Data.Repositories;

public class IdentityCardRepository : GenericRepository<IdentityCard, int>
{
    public IdentityCardRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}