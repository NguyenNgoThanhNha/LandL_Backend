using Microsoft.AspNetCore.Http;

namespace L_L.Business.Commons.Request
{
    public class UpdateInfoRequest
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? PhoneNumber { get; set; }
        public string? STK { get; set; }
        public string? Bank { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}
