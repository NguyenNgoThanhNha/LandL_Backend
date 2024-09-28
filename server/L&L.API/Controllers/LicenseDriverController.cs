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
    public class LicenseDriverController : ControllerBase
    {
        private readonly LicenseDriverService _licenseDriverService;
        private readonly UserService _userService;

        public LicenseDriverController(LicenseDriverService licenseDriverService, UserService userService)
        {
            _licenseDriverService = licenseDriverService;
            _userService = userService;
        }

        [Authorize(Roles = "Driver")]
        [HttpPatch("UpdateLicenseDriver")]
        public async Task<IActionResult> UpdateLicenseDriver([FromForm] LicenseDriverRequest request)
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
            var currentUser = await _userService.GetUserInToken(tokenValue);
            if (currentUser == null || currentUser.RoleID != 3)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "Driver not found!"
                }));
            }
            
            var licenseDriverUpdate = await _licenseDriverService.UpdateLicenseDriver(request, currentUser.UserId);
            if (licenseDriverUpdate == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "Error in update LicenseDriver!"
                }));
            }

            return Ok(ApiResult<LicenseDriverResponse>.Succeed(new LicenseDriverResponse()
            {
                data = licenseDriverUpdate
            }));
        }
    }
}
