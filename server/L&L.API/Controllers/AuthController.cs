using L_L.Business.Commons;
using L_L.Business.Commons.Request;
using L_L.Business.Commons.Response;
using L_L.Business.Exceptions;
using L_L.Business.Models;
using L_L.Business.Services;
using L_L.Business.Ultils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace L_L.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService authService;
        private readonly MailService mailService;

        public AuthController(AuthService authService, MailService mailService)
        {
            this.authService = authService;
            this.mailService = mailService;
        }

        [HttpPost("FirstStep")]
        public async Task<IActionResult> FirstStepResgisterInfo(FirstStepResquest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }
            if (req.TypeAccount == null)
            {
                return BadRequest(ApiResult<FirstStepResgisterInfoResponse>.Error(new FirstStepResgisterInfoResponse
                {
                    message = "Please select type account"
                }));
            }
            var otp = 0;
            var Password = req.Password;
            var email = req.Email;
            var link = req.Link;
            var user = await authService.GetUserByEmail(email);
            if (user != null && user.OTPCode == "0")
            {
                return BadRequest(ApiResult<FirstStepResgisterInfoResponse>.Error(new FirstStepResgisterInfoResponse
                {
                    message = "Account Already Exists"
                }));
            }

            if (user != null && user.CreateDate > DateTime.Now && user.OTPCode != "0")
            {
                return BadRequest(ApiResult<FirstStepResgisterInfoResponse>.Error(new FirstStepResgisterInfoResponse
                {
                    message = "OTP Code is not expired"
                }));
            }

            if (user == null)
            {
                otp = new Random().Next(100000, 999999);
                var href = link + req.Email;
                var mailData = new MailData()
                {
                    EmailToId = email,
                    EmailToName = "KayC",
                    EmailBody = $@"
<div style=""max-width: 400px; margin: 50px auto; padding: 30px; text-align: center; font-size: 120%; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 20px rgba(0, 0, 0, 0.1); position: relative;"">
    <img src=""https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTRDn7YDq7gsgIdHOEP2_Mng6Ym3OzmvfUQvQ&usqp=CAU"" alt=""Noto Image"" style=""max-width: 100px; height: auto; display: block; margin: 0 auto; border-radius: 50%;"">
    <h2 style=""text-transform: uppercase; color: #3498db; margin-top: 20px; font-size: 28px; font-weight: bold;"">Welcome to Team 3</h2>
    <a href=""{href}"" style=""display: inline-block; background-color: #3498db; color: #fff; text-decoration: none; padding: 10px 20px; border-radius: 5px; margin-bottom: 20px;"">Click here to verify</a>
    <div style=""font-size: 18px; color: #555; margin-bottom: 30px;"">Your OTP Code is: <span style=""font-weight: bold; color: #e74c3c;"">{otp}</span></div>
    <p style=""color: #888; font-size: 14px;"">Powered by Team 3</p>
</div>",
                    EmailSubject = "OTP Verification"
                };


                var result = await mailService.SendEmailAsync(mailData);
                if (!result)
                {
                    throw new BadRequestException("Send Email Fail");
                }
            }
            var createUserModel = new UserModel
            {
                Email = req.Email,
                OTPCode = otp.ToString(),
                Password = Password,
                UserName = req.UserName,
                FullName = req.FullName,
                Address = req.Address,
                City = req.City,
                PhoneNumber = req.Phone
            };
            var userModel = await authService.FirstStep(createUserModel, req.TypeAccount);

            if (userModel.OTPCode != otp.ToString())
            {
                var href = link + req.Email;
                var mailUpdateData = new MailData()
                {
                    EmailToId = email,
                    EmailToName = "KayC",
                    EmailBody = $@"
<div style=""max-width: 400px; margin: 50px auto; padding: 30px; text-align: center; font-size: 120%; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 20px rgba(0, 0, 0, 0.1); position: relative;"">
    <img src=""https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTRDn7YDq7gsgIdHOEP2_Mng6Ym3OzmvfUQvQ&usqp=CAU"" alt=""Noto Image"" style=""max-width: 100px; height: auto; display: block; margin: 0 auto; border-radius: 50%;"">
    <h2 style=""text-transform: uppercase; color: #3498db; margin-top: 20px; font-size: 28px; font-weight: bold;"">Welcome to Team 3</h2>
    <a href=""{href}"" style=""display: inline-block; background-color: #3498db; color: #fff; text-decoration: none; padding: 10px 20px; border-radius: 5px; margin-bottom: 20px;"">Click here to verify</a>
    <div style=""font-size: 18px; color: #555; margin-bottom: 30px;"">Your OTP Code is: <span style=""font-weight: bold; color: #e74c3c;"">{userModel.OTPCode}</span></div>
    <p style=""color: #888; font-size: 14px;"">Powered by Team 3</p>
</div>",
                    EmailSubject = "OTP Verification"
                };
                var rsUpdate = await mailService.SendEmailAsync(mailUpdateData);
                if (!rsUpdate)
                {
                    throw new BadRequestException("Send Email Fail");
                }
            }

            return Ok(ApiResult<FirstStepResgisterInfoResponse>.Succeed(new FirstStepResgisterInfoResponse
            {
                message = "Check Email and Verify OTP",
            }));
        }

        [HttpPost("SubmitOTP")]
        public async Task<IActionResult> SubmitOTP(SubmitOTPResquest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }
            var result = await authService.SubmitOTP(req);
            if (!result)
            {
                throw new BadRequestException("OTP Code is not Correct");
            }
            return Ok(ApiResult<FirstStepResgisterInfoResponse>.Succeed(new FirstStepResgisterInfoResponse
            {
                message = $"Verify Account Success for email: {req.Email}"
            }));
        }


        [HttpGet("GetTimeOTP")]
        public async Task<IActionResult> GetTimeOTP(string email)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }
            var user = await authService.GetUserByEmail(email);
            if (user == null)
            {
                return NotFound(ApiResult<GetTimeOTP>.Error(new GetTimeOTP
                {
                    message = "User Not Found"
                }));
            }

            else
            {
                DateTimeOffset utcTime = DateTimeOffset.Parse(user.CreateDate.ToString());
                return Ok(ApiResult<GetTimeOTP>.Succeed(new GetTimeOTP
                {
                    message = "Success",
                    data = utcTime
                }));

            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var loginResult = authService.SignIn(req.email, req.password);
            if (loginResult.Token == null)
            {
                var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Username or password is invalid"));
                return BadRequest(result);
            }
            
            var handler = new JwtSecurityTokenHandler();
            
            // Set the refresh token in the cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,        // Accessible only by the server
                Secure = true,          // Ensure it is only sent over HTTPS
                SameSite = SameSiteMode.Strict, // Prevent CSRF
                Expires = DateTime.UtcNow.AddDays(30) // Expiration time of 30 days
            };

            var refreshToken = handler.WriteToken(loginResult.Refresh);
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
            
            var res = new SignInResponse
            {
                message = "Sign In Successfully",
                data = handler.WriteToken(loginResult.Token)
            };
            return Ok(ApiResult<SignInResponse>.Succeed(res));
        }

        [AllowAnonymous]
        [HttpPost("login-google")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] LoginWithGGRequest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var loginResult = await authService.SignInWithGG(req);

            if (loginResult.Authenticated)
            {
                var handler = new JwtSecurityTokenHandler();
                
                // Set the refresh token in the cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,        // Accessible only by the server
                    Secure = true,          // Ensure it is only sent over HTTPS
                    SameSite = SameSiteMode.Strict, // Prevent CSRF
                    Expires = DateTime.UtcNow.AddDays(30) // Expiration time of 30 days
                };

                Response.Cookies.Append("refreshToken", handler.WriteToken(loginResult.Refresh), cookieOptions);
                
                var res = new SignInResponse
                {
                    message = "Sign In Successfully",
                    data = handler.WriteToken(loginResult.Token)
                };
                return Ok(ApiResult<SignInResponse>.Succeed(res));
            }

            return BadRequest(ApiResult<SignInResponse>.Error(new SignInResponse
            {
                message = "Error in login with Google!"
            }));
        }

        [HttpPost("Forget-Password")]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Invalid request data!"
                }));
            }

            var user = await authService.GetUserByEmail(email);
            if (user == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Account not found!"
                }));
            }

            var result = await authService.ForgotPass(user);
            if (result == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Failed to generate OTP!"
                }));
            }

            var mailUpdateData = new MailData()
            {
                EmailToId = email,
                EmailToName = "KayC",
                EmailBody = $@"
<div style=""max-width: 400px; margin: 50px auto; padding: 30px; text-align: center; font-size: 120%; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 20px rgba(0, 0, 0, 0.1); position: relative;"">
    <img src=""https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTRDn7YDq7gsgIdHOEP2_Mng6Ym3OzmvfUQvQ&usqp=CAU"" alt=""Noto Image"" style=""max-width: 100px; height: auto; display: block; margin: 0 auto; border-radius: 50%;"">
    <h2 style=""text-transform: uppercase; color: #3498db; margin-top: 20px; font-size: 28px; font-weight: bold;"">Welcome to Team 3</h2>
    <div style=""font-size: 18px; color: #555; margin-bottom: 30px;"">Your OTP Code is: <span style=""font-weight: bold; color: #e74c3c;"">{result.OTPCode}</span></div>
    <p style=""color: #888; font-size: 14px;"">Powered by Team 3</p>
</div>",
                EmailSubject = "OTP Verification"
            };
            var rsUpdate = await mailService.SendEmailAsync(mailUpdateData);
            if (!rsUpdate)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Failed to send email!"
                }));
            }

            return Ok(ApiResult<ResponseMessage>.Succeed(new ResponseMessage
            {
                message = "OTP sent successfully!"
            }));
        }

        [HttpPost("Update-Password")]
        public async Task<IActionResult> UpdatePassword([FromQuery] string email, [FromBody] UpdatePasswordRequest req)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(req.Password) || string.IsNullOrEmpty(req.ConfirmPassword))
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Invalid request data!"
                }));
            }

            if (req.Password != req.ConfirmPassword)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Passwords do not match!"
                }));
            }

            var result = await authService.UpdatePass(email, req.Password);

            if (result)
            {
                return Ok(ApiResult<ResponseMessage>.Succeed(new ResponseMessage
                {
                    message = "Password updated successfully!"
                }));
            }
            else
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Failed to update password! Please make sure verify otp code to update new password!"
                }));
            }
        }

        [HttpPost("Resend-Otp")]
        public async Task<IActionResult> ResendOtp([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Invalid request data!"
                }));
            }

            var user = await authService.GetUserByEmail(email);
            if (user == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Account not found!"
                }));
            }

            var result = await authService.ResendOtp(email);

            if (result != null)
            {
                var mailData = new MailData()
                {
                    EmailToId = email,
                    EmailToName = user.FullName,
                    EmailBody = $@"
<div style=""max-width: 400px; margin: 50px auto; padding: 30px; text-align: center; font-size: 120%; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 20px rgba(0, 0, 0, 0.1); position: relative;"">
    <img src=""https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTRDn7YDq7gsgIdHOEP2_Mng6Ym3OzmvfUQvQ&usqp=CAU"" alt=""Noto Image"" style=""max-width: 100px; height: auto; display: block; margin: 0 auto; border-radius: 50%;"">
    <h2 style=""text-transform: uppercase; color: #3498db; margin-top: 20px; font-size: 28px; font-weight: bold;"">Welcome to Team 3</h2>
    <div style=""font-size: 18px; color: #555; margin-bottom: 30px;"">Your OTP Code is: <span style=""font-weight: bold; color: #e74c3c;"">{result.OTPCode}</span></div>
    <p style=""color: #888; font-size: 14px;"">Powered by Team 3</p>
</div>",
                    EmailSubject = "Resend OTP Verification"
                };

                var emailSent = await mailService.SendEmailAsync(mailData);
                if (emailSent)
                {
                    return Ok(ApiResult<ResponseMessage>.Succeed(new ResponseMessage
                    {
                        message = "OTP resent successfully!"
                    }));
                }
                else
                {
                    return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                    {
                        message = "Failed to send email!"
                    }));
                }
            }
            else
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Failed to resend OTP!"
                }));
            }
        }

        [HttpGet("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            // Retrieve the refresh token from the cookie
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "No refresh token provided"
                }));
            }

            try
            {
                // Call the service to validate and refresh the tokens
                var loginResult = await authService.RefreshToken(refreshToken);

                if (!loginResult.Authenticated)
                {
                    return Unauthorized(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                    {
                        message = "Refresh token expired or invalid, please log in again"
                    }));
                }

                // Return the new access token
                var handler = new JwtSecurityTokenHandler();
                var res = new SignInResponse
                {
                    message = "Token refreshed successfully",
                    data = handler.WriteToken(loginResult.Token)
                };

                return Ok(ApiResult<SignInResponse>.Succeed(res));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = ex.Message
                }));
            }
        }


    }
}
