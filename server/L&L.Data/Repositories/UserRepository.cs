using L_L.Data.Base;
using L_L.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L_L.Data.Repositories
{
    public class UserRepository : GenericRepository<User, int>
    {
        public UserRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
