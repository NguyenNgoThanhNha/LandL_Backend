using System.ComponentModel.DataAnnotations;

namespace L_L.Business.Commons.Request;

public class AddOrderDetailToOrderRequest
{
    [Required(ErrorMessage = "OrderId of driver is required!")]
    public int orderId { get; set; }
    [Required(ErrorMessage = "OrderDetailId of driver is required!")]
    public int orderDetailId { get; set; }
}