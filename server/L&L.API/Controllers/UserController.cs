using L_L.Business.Commons;
using L_L.Business.Commons.Request;
using L_L.Business.Commons.Response;
using L_L.Business.Models;
using L_L.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L_L.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService userService;

        public UserController(UserService userService)
        {
            this.userService = userService;
        }

        [HttpGet("Get1")]
        [HttpGet("Get2")]
        [HttpGet("Get3")]
        public async Task<IActionResult> GetAuthor()
        {
            var currentRoute = HttpContext.Request.Path.Value.ToLower();

            // require admin for get 1
            if (currentRoute.Contains("get1"))
            {
                if (!User.IsInRole("Admin"))
                {
                    return Forbid();  // return 403 if not admin
                }
            }

            var users = userService.GetAllUser();
            return Ok(ApiResult<GetAllUserResponse>.Succeed(new GetAllUserResponse()
            {
                data = users
            }));
        }

        // Roles
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("Roles")]
        public IActionResult GetBaseRole()
        {
            var users = userService.GetAllUser();
            return Ok(ApiResult<GetAllUserResponse>.Succeed(new GetAllUserResponse()
            {
                data = users
            }));
        }

        // Policy
        [Authorize(Policy = "CustomerOnly")]
        [HttpGet("Policy")]
        public async Task<IActionResult> GetBasePolicyAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return Unauthorized(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Authorization header is missing."
                }));
            }

            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            var currentUser = await userService.GetUserInToken(tokenValue);
            return Ok(ApiResult<UserModel>.Succeed(currentUser));
        }

        // Authorization handlers
        [Authorize(Policy = "AdminHandler")]
        [HttpGet("AdminHandler")]
        public IActionResult AdminAction()
        {
            return Ok(ApiResult<ResponseMessage>.Succeed(new ResponseMessage()
            {
                message = "This action can only be accessed by Admins."
            }));
        }



        [Authorize(Roles = "Admin")]
        [HttpGet("Get-All")]
        public IActionResult Get()
        {
            var users = userService.GetAllUser();
            return Ok(ApiResult<GetAllUserResponse>.Succeed(new GetAllUserResponse()
            {
                data = users
            }));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateCost")]
        public async Task<IActionResult> UpdateCost([FromQuery] string email, [FromBody] UpdateCostRequest req)
        {
            var currentUser = await userService.GetUserByEmail(email);

            if (currentUser == null)
            {
                return NotFound(ApiResult<UpdateUserRespone>.Error(new UpdateUserRespone
                {
                    message = "User not found"
                }));
            }

            var result = await userService.UpdateCost(req.cost, email);

            if (result)
            {
                return Ok(ApiResult<UpdateUserRespone>.Succeed(new UpdateUserRespone
                {
                    message = "Update cost success"
                }));
            }

            return BadRequest(ApiResult<UpdateUserRespone>.Error(new UpdateUserRespone
            {
                message = "Error in update cost"
            }));
        }

        [HttpPatch("UpdateInfo")]
        public async Task<IActionResult> UpdateInfo(string email, [FromForm] UpdateInfoRequest req)
        {

            /*            Request.Headers.TryGetValue("Authorization", out var token);
                        token = token.ToString().Split()[1];
                        var currentUser = await _userService.GetUserInToken(token);*/

            var currentUser = await userService.GetUserByEmail(email);

            if (currentUser == null)
            {
                return NotFound(ApiResult<UpdateUserRespone>.Error(new UpdateUserRespone
                {
                    message = "User not found"
                }));
            }


            var updatedUser = await userService.UpdateInfoUser(currentUser, req);

            if (updatedUser != null)
            {
                return Ok(ApiResult<UpdateUserRespone>.Succeed(new UpdateUserRespone
                {
                    data = updatedUser,
                    message = "Update Info Success"
                }));
            }
            else
            {
                return BadRequest(ApiResult<UpdateUserRespone>.Error(new UpdateUserRespone
                {
                    message = "Invalid data provided"
                }));
            }
        }

        [HttpGet("GetProfile")]
        public async Task<IActionResult> GetMyProfile()
        {
            // Lấy token từ header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return Unauthorized(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Authorization header is missing."
                }));
            }

            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            var currentUser = await userService.GetUserInToken(tokenValue);
            if (currentUser == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "User not found!"
                }));
            }
            return Ok(ApiResult<UpdateUserRespone>.Succeed(new UpdateUserRespone()
            {
                message = "Get User Info Success",
                data = currentUser
            }));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetRoleForAdmin")]
        public async Task<IActionResult> GetAllRole()
        {
            // Lấy token từ header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return Unauthorized(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Authorization header is missing."
                }));
            }

            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            var currentUser = await userService.GetUserInToken(tokenValue);
            if (currentUser == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "Admin not found!"
                }));
            }
            
            
            
            return Ok();
        }

    }
}
