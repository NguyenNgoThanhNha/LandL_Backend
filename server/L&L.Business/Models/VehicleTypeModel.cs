using L_L.Data.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Business.Models
{
    public class VehicleTypeModel
    {
        public int VehicleTypeId { get; set; }

        public string VehicleTypeName { get; set; }

        [Column(TypeName = "decimal(19, 2)")]
        public decimal BaseRate { get; set; }

        // Navigation property
        public ICollection<ShippingRate> ShippingRates { get; set; }
        public ICollection<VehiclePackageRelation> VehiclePackageRelations { get; set; }
    }
}
