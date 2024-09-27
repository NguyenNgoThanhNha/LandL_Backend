using L_L.Business.Commons;
using L_L.Business.Commons.Response;
using L_L.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace L_L.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleTypeController : ControllerBase
    {
        private readonly VehicleTypeService _vehicleTypeService;

        public VehicleTypeController(VehicleTypeService vehicleTypeService)
        {
            _vehicleTypeService = vehicleTypeService;
        }

        [HttpGet("GetAllVehicleType")]
        public async Task<IActionResult> GetAllVehicleTypeService()
        {
            var list = await _vehicleTypeService.GetAllVehiType();
            return Ok(ApiResult<ListVehicleTypeResponse>.Succeed(new ListVehicleTypeResponse()
            {
                data = list
            }));
        }
    }
}
