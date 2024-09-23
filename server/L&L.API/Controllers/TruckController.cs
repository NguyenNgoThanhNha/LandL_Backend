using L_L.Business.Commons;
using L_L.Business.Commons.Response;
using L_L.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace L_L.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TruckController : ControllerBase
    {
        private readonly TruckSevice _truckSevice;

        public TruckController(TruckSevice truckSevice)
        {
            _truckSevice = truckSevice;
        }

        [HttpGet("GetTruckOfUser")]
        public async Task<IActionResult> GetTruckOfUser([FromQuery] string userId)
        {
            if (userId == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "User Id is required!"
                }));
            }
            var Truck = await _truckSevice.GetTruckByUserId(userId);
            return Ok(ApiResult<GetTruckByUserResponse>.Succeed(new GetTruckByUserResponse()
            {
                data = Truck
            }));
        }
    }
}
