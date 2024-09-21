using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Data.Entities
{
    [Table("VehiclePackageRelation")]
    public class VehiclePackageRelation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RelationId { get; set; }

        [ForeignKey("VehicleType")]
        public int VehicleTypeId { get; set; }

        [ForeignKey("PackageType")]
        public int PackageTypeId { get; set; }

        // Navigation properties
        public VehicleType VehicleType { get; set; }
        public PackageType PackageType { get; set; }
    }

}
