using L_L.Data.Base;
using L_L.Data.Entities;

namespace L_L.Data.Repositories
{
    public class UserRoleRepository : GenericRepository<UserRole, int>
    {
        public UserRoleRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
