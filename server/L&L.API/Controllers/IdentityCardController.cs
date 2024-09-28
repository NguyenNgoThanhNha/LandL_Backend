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
    public class IdentityCardController : ControllerBase
    {
        private readonly IdentityCardService _identityCardService;
        private readonly UserService _userService;

        public IdentityCardController(IdentityCardService identityCardService, UserService userService)
        {
            _identityCardService = identityCardService;
            _userService = userService;
        }

        [Authorize(Roles = "Driver")]
        [HttpPatch("UpdateIdentityCard")]
        public async Task<IActionResult> UpdateIdentityCard([FromForm] IdentityCardRequest request)
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

            var identityCardUpdate = await _identityCardService.UpdateIdentityCard(request, currentUser.UserId);
            if (identityCardUpdate == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "Error in update IdentityCard!"
                }));
            }
            
            return Ok(ApiResult<IdentityCardResponse>.Succeed(new IdentityCardResponse()
            {
                data = identityCardUpdate
            }));
        }
    }
}
