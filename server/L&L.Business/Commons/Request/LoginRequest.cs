using System.ComponentModel.DataAnnotations;

namespace L_L.Business.Commons.Request
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required")]
        public string email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string password { get; set; }
    }
}
