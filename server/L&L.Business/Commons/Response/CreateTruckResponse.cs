using L_L.Business.Models;

namespace L_L.Business.Commons.Response;

public class CreateTruckResponse
{
    public string message { get; set; }
    public TruckModel data { get; set; }
}