namespace L_L.Business.Models;

public class IdentityCardModel
{
    public int IdentityCardId { get; set; }
    public string? id { get; set; }
    public string? name { get; set; }
    public DateTime? dob { get; set; }
    public string? home { get; set; }
    public string? address { get; set; }
    public string? sex { get; set; }
    public string? nationality { get; set; }
    public DateTime? doe { get; set; }
    public string? type { get; set; }
    public string? type_new { get; set; }
    public string? address_entities { get; set; }
    public string? features { get; set; }
    public DateTime? issue_date { get; set; }
    public string? imageFront { get; set; }
    public string? imageBack{ get; set; }
    public int UserId { get; set; }
}