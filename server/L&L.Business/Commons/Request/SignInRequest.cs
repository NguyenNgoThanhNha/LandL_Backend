using System.ComponentModel.DataAnnotations;

namespace L_L.Business.Commons.Request
{
    public class SignInRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password length can't be more than 100.")]
        public string Password { get; set; } = null!;

        [MaxLength(255)]
        public string? UserName { get; set; }

        [MaxLength(50)]
        public string? FullName { get; set; }

        public string? Avatar { get; set; }

        [MaxLength(100)]
        public string? Gender { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? PhoneNumber { get; set; }

        public string? TypeAccount { get; set; }
    }
}
