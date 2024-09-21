using L_L.Business.Models;

namespace L_L.Business.Commons.Response
{
    public class SearchServiceResponse
    {
        public string message { get; set; }
        public List<PacketTypeModel> data { get; set; }
    }
}
