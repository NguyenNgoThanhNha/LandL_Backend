using AutoMapper;
using L_L.Business.Models;
using L_L.Data.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

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

        public async Task<List<VehicleTypeModel>> GetAllVehiType()
        {
            var list = unitOfWorks.VehicleTypeRepository.GetAll();
            return mapper.Map<List<VehicleTypeModel>>(list);
        }
    }
}
