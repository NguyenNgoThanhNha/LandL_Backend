using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Data.Entities
{
    [Table("OrderTracking")]
    public class OrderTracking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderTrackingId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? UpdateDate { get; set; }
        public string? Location { get; set; } = string.Empty;
        public bool IsDelivered { get; set; } = false;
        public DateTime? DeliveryConfirmedDate { get; set; }
        public string? ConfirmImage { get; set; }

        [Required]
        [ForeignKey("OrderInfo")]
        public int OrderId { get; set; }
        public virtual Order OrderInfo { get; set; }
    }

}
