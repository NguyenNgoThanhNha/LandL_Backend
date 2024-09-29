using L_L.Business.Models;

namespace L_L.Business.Commons.Response;

public class GetAllTruckPaginationResponse
{
    public List<TruckModel> data { get; set; }
    public Pagination pagination { get; set; }
}