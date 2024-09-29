using L_L.Business.Models;

namespace L_L.Business.Commons.Response;

public class GetAllUserPaginationResponse
{
    public List<UserModel> data { get; set; }
    public Pagination pagination { get; set; }
}

public class Pagination
{
    public int page { get; set; }
    public int totalPage { get; set; }
    public int totalCount { get; set; }
}