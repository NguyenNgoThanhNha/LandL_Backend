using L_L.Business.Models;
using Microsoft.AspNetCore.Http;

namespace L_L.Business.Commons.Request
{
    public class UpdateProductInfoRequest
    {
        public string orderDetailId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        
        public IFormFile? Image { get; set; }
    }
}
