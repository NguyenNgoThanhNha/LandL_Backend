using System.ComponentModel.DataAnnotations;

namespace L_L.Business.Commons.Request;

public class CreateOrderRequest
{
    [Required]
    public string From { get; set; }
    [Required]
    public string To { get; set; }
    [Required]
    public string longFrom { get; set; }
    [Required]
    public string latFrom { get; set; }
    [Required]
    public string longTo { get; set; }
    [Required]
    public string latTo { get; set; }
    
    [Required(ErrorMessage = "Distance is required.")]
    public decimal Distance { get; set; }
    
    [Required]
    public DateTime PickupTime { get; set; } // Pickup time
    [Required]
    public decimal Weight { get; set; }
    [Required]
    public decimal Length { get; set; }
    [Required]
    public decimal Width { get; set; }
    [Required]
    public decimal Height { get; set; }
    
    public string? Type { get; set; }
    public string Email { get; set; }

    public decimal TotalAmount { get; set; }

    public int VehicleTypeId { get; set; }
    
    public Dictionary<int, decimal> listCost { get; set; }
}
