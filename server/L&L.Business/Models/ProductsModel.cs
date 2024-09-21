namespace L_L.Business.Models
{
    public class ProductsModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? Quantity { get; set; }
        public string ProductDescription { get; set; }
        public string? TotalDismension { get; set; } // length * width * height
        public string? Weight { get; set; }
        public string? Image { get; set; }
        public int? SenderId { get; set; }
    }
}
