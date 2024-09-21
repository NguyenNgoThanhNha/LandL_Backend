using L_L.Business.Commons.Request;
using L_L.Business.Ultils;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;

namespace L_L.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayOsController : ControllerBase
    {
        private readonly PayOSSetting payOSSetting;

        public PayOsController(PayOSSetting payOSSetting)
        {
            this.payOSSetting = payOSSetting;
        }
        [HttpPost("create-payment-link")]
        public async Task<IActionResult> Create([FromBody] PayOsRequest req)
        {

            var payOS = new PayOS(payOSSetting.ClientId, payOSSetting.ApiKey, payOSSetting.ChecksumKey);

            var domain = payOSSetting.Domain;

            var paymentLinkRequest = new PaymentData(
                orderCode: int.Parse(DateTimeOffset.Now.ToString("ffffff")),
                amount: 20000,
                description: "Thanh toan don hang",
                items: [new("Mì tôm hảo hảo ly", 1, 2000)],
                returnUrl: $"{domain}/${req.returnUrl}",
                cancelUrl: $"{domain}/${req.cancelUrl}"
            );
            var response = await payOS.createPaymentLink(paymentLinkRequest);

            Response.Headers.Append("Location", response.checkoutUrl);
            return Ok(response.checkoutUrl);
        }

        [HttpGet("receive-webhook")]
        public IActionResult GetResultPayOsOrder()
        {
            return Ok();
        }

    }
}
