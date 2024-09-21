using L_L.Data.UnitOfWorks;

namespace L_L.Business.Services
{
    public class ProductService
    {
        private readonly UnitOfWorks unitOfWorks;

        public ProductService(UnitOfWorks unitOfWorks)
        {
            this.unitOfWorks = unitOfWorks;
        }
    }
}
