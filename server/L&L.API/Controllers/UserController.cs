using L_L.Business.Commons;
using L_L.Business.Commons.Request;
using L_L.Business.Commons.Response;
using L_L.Business.Models;
using L_L.Business.Services;
using L_L.Business.Ultils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L_L.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService userService;
        private readonly MailService _mailService;
        private readonly GuessService _guessService;

        public UserController(UserService userService, MailService mailService, GuessService guessService)
        {
            this.userService = userService;
            _mailService = mailService;
            _guessService = guessService;
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

            var users = await userService.GetAllUser();
            return Ok(ApiResult<GetAllUserResponse>.Succeed(new GetAllUserResponse()
            {
                data = users
            }));
        }

        // Roles
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("Roles")]
        public async Task<IActionResult> GetBaseRole()
        {
            var users = await userService.GetAllUser();
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
        public async Task<IActionResult> Get([FromQuery] int page = 1)
        {
            var users = await userService.GetAllUser(page);
            return Ok(ApiResult<GetAllUserPaginationResponse>.Succeed(new GetAllUserPaginationResponse()
            {
                data = users.data,
                pagination = users.pagination
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

            var listUserRole = await userService.GetAllRole();
            if (listUserRole == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "User not found!"
                }));
            }
            
            return Ok(ApiResult<ListUserRoleResponse>.Succeed(new ListUserRoleResponse()
            {
                data = listUserRole
            }));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetUserAge")]
        public async Task<IActionResult> GetUserAge()
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
            
            var listUserAge = await userService.GetUserAge();
            if (listUserAge == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "User not found!"
                }));
            }
            
            return Ok(ApiResult<ListUserRoleResponse>.Succeed(new ListUserRoleResponse()
            {
                data = listUserAge
            }));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetUserTypeLogin")]
        public async Task<IActionResult> GetUserTypeLogin()
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
            
            var listUserTypeLogin = await userService.GetUserTypeLoginByMonth();
            if (listUserTypeLogin == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "User not found!"
                }));
            }
            
            return Ok(ApiResult<GetUserTypeLoginResponse>.Succeed(new GetUserTypeLoginResponse()
            {
                data = listUserTypeLogin
            }));
        }

        [HttpGet("SendDetailCost")]
        public async Task<IActionResult> SendDetailCost([FromQuery] string email)
        {
            var mailData = new MailData()
            {
                EmailToId = email,
                EmailToName = "KayC",
                EmailBody = $@"
<div style=""max-width: 600px; margin: 50px auto; padding: 30px; text-align: center; font-size: 120%; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 20px rgba(0, 0, 0, 0.1); position: relative;"">
    <h2 style=""text-transform: uppercase; color: #3498db; margin-top: 20px; font-size: 28px; font-weight: bold;"">Bảng giá sản phẩm</h2>
    <p style=""font-size: 18px; color: #555; margin-bottom: 30px;"">Chúng tôi gửi bạn bảng giá của 2 loại sản phẩm bên dưới:</p>
    
    <div style=""margin-bottom: 20px;"">
        <h3 style=""color: #3498db;"">Bảng giá cước vận chuyển xe tải:</h3>
        <img src=""https://res.cloudinary.com/dgezepi0f/image/upload/v1728304438/c8soj0baqqhzs33odl2a.png"" alt=""Bảng giá sản phẩm 1"" style=""max-width: 100%; height: auto; display: block; margin: 0 auto;"">
    </div>
    
    <div>
        <h3 style=""color: #3498db;"">Bảng quy chuẩn hàng hóa vận chuyển xe tải:</h3>
        <img src=""https://res.cloudinary.com/dgezepi0f/image/upload/v1728304449/mxvewtx7xj03lrysvany.png"" alt=""Bảng giá sản phẩm 2"" style=""max-width: 100%; height: auto; display: block; margin: 0 auto;"">
    </div>

    <p style=""color: #888; font-size: 14px; margin-top: 30px;"">Powered by Team L&L</p>
</div>",
                EmailSubject = "Bảng giá sản phẩm"
            };
            var resultGuess = await _guessService.CreateNewGuess(email);
            var result = await _mailService.SendEmailAsync(mailData, true);
            if (!result)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "Send email fail!"
                }));
            }

            return Ok(ApiResult<ResponseMessage>.Succeed(new ResponseMessage()
            {
                message = "Send email successfully!"
            }));
        }

        [HttpPost("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount()
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

            var result = await userService.DeleteAccount(currentUser);
            if (!result)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "Error in delete user!"
                }));
            }

            return Ok(ApiResult<ResponseMessage>.Succeed(new ResponseMessage()
            {
                message = "Delete user successfully!"
            }));
        }

    }
}
