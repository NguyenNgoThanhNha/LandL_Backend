using L_L.Data.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Business.Models
{
    public class ShippingRateModel
    {
        public int RateId { get; set; }

        [ForeignKey("VehicleType")]
        public int VehicleTypeId { get; set; }

        public int DistanceFrom { get; set; }

        public int? DistanceTo { get; set; } // Khoảng cách tối đa có thể là NULL

        [Column(TypeName = "decimal(19, 2)")]
        public decimal RatePerKM { get; set; }

        // Navigation property
        public VehicleType VehicleType { get; set; }
    }
}
