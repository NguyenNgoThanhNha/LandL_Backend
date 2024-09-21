using L_L.Data.UnitOfWorks;

namespace L_L.Business.Services
{
    public class DeliveryInfoService
    {
        private readonly UnitOfWorks unitOfWorks;

        public DeliveryInfoService(UnitOfWorks unitOfWorks)
        {
            this.unitOfWorks = unitOfWorks;
        }
    }
}
