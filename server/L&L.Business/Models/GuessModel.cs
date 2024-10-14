using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Business.Models;

public class GuessModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int GuessId { get; set; }
    public string email { get; set; }
    
    public string? phone { get; set; }
    
    public int? age { get; set; }
    
    public string? priorityAddress { get; set; }
    
    public string? licenseType { get; set; }
    
    public int RoleID { get; set; }
    public string status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; } 
}