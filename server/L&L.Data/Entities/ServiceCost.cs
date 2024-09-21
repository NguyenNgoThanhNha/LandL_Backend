using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L_L.Data.Entities;
[Table("ServiceCost")]
public class ServiceCost
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ServiceCostId { get; set; }

    public int VehicleTypeId { get; set; }

    public string Amount { get; set; }

    [ForeignKey("OrderDetailPrice")]
    public int OrderDetailId { get; set; }
    public virtual OrderDetails OrderDetail { get; set; }
}