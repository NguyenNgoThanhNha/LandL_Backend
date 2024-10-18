using Microsoft.AspNetCore.Http;

namespace L_L.Business.Commons.Request;

public class LicenseDriverRequest
{
    public string? id { get; set; }
    public string? name { get; set; }
    public DateTime? dob { get; set; }
    public string? nation { get; set; }
    public string? address { get; set; }
    public string? place_issue { get; set; }
    public DateTime? date { get; set; }
    public string? doe { get; set; }
    public string? classLicense { get; set; }
    public string? type { get; set; }
    public IFormFile? imageFront { get; set; }
    public IFormFile? imageBack{ get; set; }
}