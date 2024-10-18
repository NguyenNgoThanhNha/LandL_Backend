using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Data.Entities;
[Table("LicenseDriver")]
public class LicenseDriver
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LicenseDriverId { get; set; }
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
    public string? imageFront { get; set; }
    public string? imageBack{ get; set; }
    [ForeignKey("UserLicense")]
    public int UserId { get; set; }
    public virtual User UserLicense { get; set; }
}