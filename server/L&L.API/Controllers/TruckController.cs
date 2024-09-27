using L_L.Business.Commons;
using L_L.Business.Commons.Request;
using L_L.Business.Commons.Response;
using L_L.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace L_L.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TruckController : ControllerBase
    {
        private readonly TruckSevice _truckSevice;
        private readonly UserService _userService;

        public TruckController(TruckSevice truckSevice, UserService userService)
        {
            _truckSevice = truckSevice;
            _userService = userService;
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
        [Authorize(Roles = "Driver")]
        [HttpPost("CreateTruck")]
        public async Task<IActionResult> CreateTruckForUser([FromBody] TruckRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }
            
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return Unauthorized(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Authorization header is missing."
                }));
            }

            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            var currentUser = await _userService.GetUserInToken(tokenValue);
            if (currentUser == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "User not found!"
                }));
            }

            var truckCreate = await _truckSevice.CreateTruck(request, currentUser.UserId);
            if (truckCreate == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "Error in create Truck!"
                }));
            }
            return Ok(ApiResult<CreateTruckResponse>.Succeed(new CreateTruckResponse()
            {
                message = "Create truck successfully!",
                data = truckCreate
            }));
        }

        [Authorize(Roles = "Driver")]
        [HttpPatch("UpdateTruck/{id}")]
        public async Task<IActionResult> UpdateTruckForUser([FromBody] TruckRequest request, [FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }
            
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return Unauthorized(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Authorization header is missing."
                }));
            }

            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            var currentUser = await _userService.GetUserInToken(tokenValue);
            if (currentUser == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "User not found!"
                }));
            }

            var truckUpdate = await _truckSevice.UpdateTruck(request, int.Parse(id));
            if (truckUpdate == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "Error in update Truck!"
                }));
            }
            return Ok(ApiResult<CreateTruckResponse>.Succeed(new CreateTruckResponse()
            {
                message = "Update truck successfully!",
                data = truckUpdate
            }));
        }
    }
}
