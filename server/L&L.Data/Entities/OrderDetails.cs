using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Data.Entities
{
    public class OrderDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderDetailId { get; set; }
        public int OrderDetailCode { get; set; }
        public int? Quantity { get; set; }
        public string? PaymentMethod { get; set; } = string.Empty;
        public decimal? UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public int? VehicleTypeId { get; set; }

        [ForeignKey("UserOrder")]
        public int? SenderId { get; set; }
        public virtual User UserOrder { get; set; }

        [ForeignKey("OrderDetailInfo")]
        public int? OrderId { get; set; }
        public virtual Order OrderInfo { get; set; }

        [ForeignKey("OrderProduct")]
        public int? ProductId { get; set; }
        public virtual Product ProductInfo { get; set; }

        [ForeignKey("OrderTruck")]
        public int? TruckId { get; set; }
        public virtual Truck TruckInfo { get; set; }

        [ForeignKey("OrderDelivery")]
        public int? DeliveryInfoId { get; set; }
        public virtual DeliveryInfo DeliveryInfoDetail { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

}
