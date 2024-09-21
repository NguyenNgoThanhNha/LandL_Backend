using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Data.Entities
{
    [Table("VehicleType")]
    public class VehicleType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VehicleTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string VehicleTypeName { get; set; } // số tấn của xe

        [Column(TypeName = "decimal(19, 2)")]
        public decimal BaseRate { get; set; }

        // Navigation property
        public ICollection<ShippingRate> ShippingRates { get; set; }
        public ICollection<VehiclePackageRelation> VehiclePackageRelations { get; set; }
    }

}
