using System.ComponentModel.DataAnnotations;

namespace L_L.Business.Commons.Request
{
    public class AcceptDriverRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Order ID is required")]
        public string orderId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Order Detail ID is required")]
        public string orderDetailId { get; set; }
    }

}
