namespace L_L.Business.Commons.Request;

public class CollectInfoCustomerRequest
{
    public string email { get; set; }
    
    public string? phone { get; set; }
    
    public int? age { get; set; }
    
    public string? priorityAddress { get; set; }
    
    public string? licenseType { get; set; }
}