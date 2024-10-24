using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Data.Entities
{
    [Table("Transaction")]
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }
        public decimal? Amount { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
        public string? imagePay { get; set; }
        public string Status { get; set; }

        [ForeignKey("Driver")]
        public int DriverId { get; set; }
        public virtual User Driver { get; set; }

        [ForeignKey("Admin")]
        public int AdminId { get; set; }
        public virtual User Admin { get; set; }
    }
}
