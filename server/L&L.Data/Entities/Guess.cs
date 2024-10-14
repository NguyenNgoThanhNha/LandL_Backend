using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Data.Entities;
[Table("Guess")]
public class Guess
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int GuessId { get; set; }
    public string email { get; set; }
    
    public string? phone { get; set; }
    
    public int? age { get; set; }
    
    public string? priorityAddress { get; set; }
    
    public string? licenseType { get; set; }
    
    [ForeignKey("GuessRole")]
    public int RoleID { get; set; }
    public virtual UserRole GuessRole { get; set; }
    
    public string status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}