using L_L.Business.Commons;
using L_L.Business.Commons.Response;
using L_L.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace L_L.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacketTypeController : ControllerBase
    {
        private readonly PackageTypeService _packageTypeService;

        public PacketTypeController(PackageTypeService packageTypeService)
        {
            _packageTypeService = packageTypeService;
        }
        [HttpGet("GetAllPacketType")]
        public async Task<IActionResult> GetAllPacketType()
        {
            var listPackageType = await _packageTypeService.GetAllPackageType();
            return Ok(ApiResult<ListPackageTypeResponse>.Succeed(new ListPackageTypeResponse()
            {
                data = listPackageType
            }));
        }
    }
}
