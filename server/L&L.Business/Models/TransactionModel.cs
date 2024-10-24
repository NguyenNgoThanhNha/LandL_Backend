namespace L_L.Business.Models;

public class TransactionModel
{
    public int TransactionId { get; set; }
    public decimal? Amount { get; set; }
    public string? Description { get; set; }
    public string? Note { get; set; }
    public string? imagePay { get; set; }
    public string Status { get; set; }
    public int DriverId { get; set; }
    public int AdminId { get; set; }
}