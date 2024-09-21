using AutoMapper;
using L_L.Data.UnitOfWorks;

namespace L_L.Business.Services
{
    public class VehicleTypeService
    {
        private readonly UnitOfWorks unitOfWorks;
        private readonly IMapper mapper;

        public VehicleTypeService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            this.unitOfWorks = unitOfWorks;
            this.mapper = mapper;
        }

    }
}
