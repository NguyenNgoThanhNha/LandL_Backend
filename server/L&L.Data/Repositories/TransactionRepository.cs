using L_L.Data.Base;
using L_L.Data.Entities;

namespace L_L.Data.Repositories;

public class TransactionRepository : GenericRepository<Transaction, int>
{
    public TransactionRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}