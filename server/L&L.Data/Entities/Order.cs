using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Data.Entities
{
    [Table("Order")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }
        
        public int OrderCode { get; set; }

        [Required]
        public decimal? TotalAmount { get; set; }

        public int? OrderCount { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? Notes { get; set; }

        [ForeignKey("OrderDriver")]
        public int? DriverId { get; set; }
        public virtual User OrderDriver { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? EndDate { get; set; }
    }


}
