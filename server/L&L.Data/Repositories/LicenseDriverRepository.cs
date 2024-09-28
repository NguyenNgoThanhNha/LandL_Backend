﻿using L_L.Data.Base;
using L_L.Data.Entities;

namespace L_L.Data.Repositories;

public class LicenseDriverRepository : GenericRepository<LicenseDriver, int>
{
    public LicenseDriverRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}