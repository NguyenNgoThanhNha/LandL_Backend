using System.ComponentModel.DataAnnotations;

namespace L_L.Business.Commons.Request
{
    public class SubmitOTPResquest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
        public string OTP { get; set; }
    }
}
