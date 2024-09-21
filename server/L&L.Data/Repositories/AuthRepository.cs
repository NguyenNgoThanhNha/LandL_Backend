using L_L.Data.Base;
using L_L.Data.Entities;

namespace L_L.Data.Repositories
{
    public class AuthRepository : GenericRepository<User, int>
    {
        public AuthRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
