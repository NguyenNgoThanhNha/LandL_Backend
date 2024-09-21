using System.ComponentModel.DataAnnotations;
using L_L.Business.Models;

namespace L_L.Business.Commons.Request
{
    public class UpdateDeliveryInfoRequest
    {
        [Required(ErrorMessage = "Order Detail Id is required!")]
        public string orderDetailId { get; set; }
        [Required(ErrorMessage = "Deivery Info Id is required!")]
        public string deliveryInfoId { get; set; }
        public string? receiverName { get; set; }
        public string? senderName { get; set; }
        public string? receiverPhone { get; set; }
        public string? senderPhone { get; set; }
        public string? deliveryLocaTion { get; set; }
        public string? pickUpLocation { get; set; }
    }
}
