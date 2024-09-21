using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Data.Entities
{
    [Table("Product")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? Quantity { get; set; }
        public string? ProductDescription { get; set; }
        public string TotalDismension { get; set; } // length * width * height
        public string Weight { get; set; }
        public string? Image { get; set; }
        public int? SenderId { get; set; }
    }
}
