using L_L.Business.Models;

namespace L_L.Business.Commons.Response;

public class GetOrderDriverResponse
{
    public List<OrderDetailsModel> data { get; set; }
}