namespace L_L.Business.Models;

public class LicenseDriverModel
{
    public int LicenseDriverId { get; set; }
    public string? id { get; set; }
    public string? name { get; set; }
    public DateTime? dob { get; set; }
    public string? nation { get; set; }
    public string? address { get; set; }
    public string? place_issue { get; set; }
    public DateTime? date { get; set; }
    public DateTime? doe { get; set; }
    public string? classLicense { get; set; }
    public string? type { get; set; }
    public string? imageFront { get; set; }
    public string? imageBack{ get; set; }
    public int UserId { get; set; }
}