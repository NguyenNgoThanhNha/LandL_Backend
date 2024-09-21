using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Data.Entities
{
    [Table("Truck")]
    public class Truck
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TruckId { get; set; }

        [Required]
        public string TruckName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        [Required]
        public string PlateCode { get; set; } = string.Empty;

        [Required]
        public string Color { get; set; } = string.Empty;

        public int? TotalBill { get; set; }

        [Required]
        public string Manufacturer { get; set; } = string.Empty;

        [Required]
        public string VehicleModel { get; set; } = string.Empty;

        [Required]
        public string FrameNumber { get; set; } = string.Empty;

        [Required]
        public string EngineNumber { get; set; } = string.Empty;

        [Required]
        public string LoadCapacity { get; set; } = string.Empty;

        [Required]
        public decimal DimensionsLength { get; set; }
        public decimal DimensionsWidth { get; set; }
        public decimal DimensionsHeight { get; set; }

        [ForeignKey("VehicleTypeTruckType")]
        public int VehicleTypeId { get; set; }
        public virtual VehicleType TruckType { get; set; }

        [ForeignKey("TruckUser")]
        public int UserId { get; set; }
        public virtual User TruckUser { get; set; }

    }
}
