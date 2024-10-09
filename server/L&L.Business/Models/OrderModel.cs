using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using L_L.Data.Entities;

namespace L_L.Business.Models;

public class OrderModel
{
    public int OrderId { get; set; }
    public int OrderCode { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? DriverAmount { get; set; }
    public decimal? SystemAmount { get; set; }
    public decimal? VAT { get; set; }
    public int? OrderCount { get; set; }
    public string? DismenSion { get; set; }
    public string Status { get; set; } = string.Empty;

    public string? Notes { get; set; }

    [ForeignKey("OrderDriver")]
    public int? DriverId { get; set; }
    public virtual User OrderDriver { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime EndDate { get; set; }
}