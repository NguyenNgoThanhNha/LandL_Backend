using System.ComponentModel.DataAnnotations;
using L_L.Business.Ultils;
using Microsoft.AspNetCore.Http;

namespace L_L.Business.Commons.Request;

public class UpdateStatusTransactionRequest
{
    [Required(ErrorMessage = "Status is required")]
    public StatusEnums status { get; set; }
    [Required(ErrorMessage = "TransactionId is required")]
    public int transactionId { get; set; }
    public IFormFile? image { get; set; }
}