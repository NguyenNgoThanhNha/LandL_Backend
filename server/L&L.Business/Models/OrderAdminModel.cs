namespace L_L.Business.Models;

public class OrderAdminModel
{
    public OrderModel Order { get; set; }
    public List<OrderDetailsModel> OrderDetails { get; set; }
}