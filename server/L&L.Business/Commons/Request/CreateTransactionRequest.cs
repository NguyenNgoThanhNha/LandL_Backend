namespace L_L.Business.Commons.Request;

public class CreateTransactionRequest
{
    public string amount { get; set; }
    public string? description { get; set; }
    public string? note { get; set; }
}