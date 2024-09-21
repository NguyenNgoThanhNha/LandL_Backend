namespace L_L.Business.Models
{
    public class DeliveryInfoModel
    {
        public int DeliveryInfoId { get; set; }
        public string ReceiverName { get; set; }
        public string SenderName { get; set; }
        public string ReceiverPhone { get; set; }
        public string SenderPhone { get; set; }
        public string DeliveryLocaTion { get; set; }
        public string? LongDelivery { get; set; }
        public string? LatDelivery { get; set; }
        public string PickUpLocation { get; set; }
        public string? LongPickUp { get; set; }
        public string? LatPickUp { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? RecieveDate { get; set; } // ngày nhận hàng
        public DateTime? ExpectedRecieveDate { get; set; } // ngày dự kiến nhận hàng
        public DateTime? ExpectedDeliveryDate { get; set; } // ngày dự kiến giao hàng
        public DateTime? DeliveryDate { get; set; } // ngày giao hàng
    }
}
