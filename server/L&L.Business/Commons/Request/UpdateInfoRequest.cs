using Microsoft.AspNetCore.Http;

namespace L_L.Business.Commons.Request
{
    public class UpdateInfoRequest
    {
        public string? userName { get; set; }
        public string? passWord { get; set; }
        public string? fullName { get; set; }
        public string? city { get; set; }
        public string? address { get; set; }
        public string? gender { get; set; }
        public DateTime? birthDate { get; set; }
        public string? phoneNumber { get; set; }
        public string? STK { get; set; }
        public string? Bank { get; set; }
        public IFormFile? avatar { get; set; }
    }
}
