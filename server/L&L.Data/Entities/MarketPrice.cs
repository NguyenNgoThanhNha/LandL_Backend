using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Data.Entities
{
    [Table("MarketPrice")]
    public class MarketPrice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MarketPriceId { get; set; }

        public int point { get; set; }

        public double price { get; set; }
    }
}
