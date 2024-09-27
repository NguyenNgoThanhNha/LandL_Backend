using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using L_L.Data.Entities;

namespace L_L.Business.Commons.Request;

public class TruckRequest
{
    [Required(ErrorMessage = "TruckName is required")]
    public string TruckName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "PlateCode is required")]
    public string PlateCode { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Color is required")]
    public string Color { get; set; } = string.Empty;

    public int? TotalBill { get; set; }
    
    [Required(ErrorMessage = "Manufacturer is required")]
    public string Manufacturer { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "VehicleModel is required")]
    public string VehicleModel { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "FrameNumber is required")]
    public string FrameNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "EngineNumber is required")]
    public string EngineNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "LoadCapacity is required")]
    public string LoadCapacity { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "DimensionsLength is required")]
    public decimal DimensionsLength { get; set; }
    
    [Required(ErrorMessage = "DimensionsWidth is required")]
    public decimal DimensionsWidth { get; set; }
    
    [Required(ErrorMessage = "DimensionsHeight is required")]
    public decimal DimensionsHeight { get; set; }
    
    [Required(ErrorMessage = "VehicleTypeId is required")]
    public int VehicleTypeId { get; set; }
    
    [Required(ErrorMessage = "UserId is required")]
    public int UserId { get; set; }
}