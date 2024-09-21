using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Business.Models
{
    public class PacketTypeModel
    {
        public int PackageTypeId { get; set; }

        [Column(TypeName = "decimal(19, 2)")]
        public decimal WeightLimit { get; set; }

        [Column(TypeName = "decimal(19, 2)")]
        public decimal LengthMin { get; set; }  // Minimum Length

        [Column(TypeName = "decimal(19, 2)")]
        public decimal LengthMax { get; set; }  // Maximum Length

        [Column(TypeName = "decimal(19, 2)")]
        public decimal WidthMin { get; set; }   // Minimum Width

        [Column(TypeName = "decimal(19, 2)")]
        public decimal WidthMax { get; set; }   // Maximum Width

        [Column(TypeName = "decimal(19, 2)")]
        public decimal HeightMin { get; set; }  // Minimum Height

        [Column(TypeName = "decimal(19, 2)")]
        public decimal HeightMax { get; set; }  // Maximum Height

        public int VehicleRangeMin { get; set; }  // Minimum vehicle weight limit

        public int VehicleRangeMax { get; set; }  // Maximum vehicle weight limit
    }
}
