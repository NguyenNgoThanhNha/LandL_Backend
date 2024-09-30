using L_L.Business.Models;

namespace L_L.Business.Commons.Response;

public class GetAllOrderPaginationResponse
{
    public List<OrderDetailsModel> data { get; set; }
    public Pagination pagination { get; set; }
}