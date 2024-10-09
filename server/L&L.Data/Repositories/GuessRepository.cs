using L_L.Data.Base;
using L_L.Data.Entities;

namespace L_L.Data.Repositories;

public class GuessRepository : GenericRepository<Guess, int>
{
    public GuessRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}