using L_L.API.ZaloPayHelper;
using L_L.API.ZaloPayHelper.Crypto;
using L_L.Business.Ultils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace L_L.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ZaloPaySetting zaloPaySetting;

        public PaymentController(ZaloPaySetting zaloPaySetting)
        {
            this.zaloPaySetting = zaloPaySetting;
        }

        [HttpPost("payment")]
        public async Task<IActionResult> CreatePayment()
        {
            try
            {
                Random rnd = new Random();
                var embed_data = new
                {
                    redirecturl = "https://www.facebook.com/nguyenngothanhnha?locale=vi_VN"
                };
                var items = new[] { new { } };

                // Generate a random order ID
                var app_trans_id = rnd.Next(1000000);

                // Prepare the request parameters
                var param = new Dictionary<string, string>
               {
                   { "app_id", zaloPaySetting.app_id },
                   { "app_user", "user123" },
                   { "app_time", Utils.GetTimeStamp().ToString() },
                   { "amount", "50000" },
                   { "app_trans_id", DateTime.Now.ToString("yyMMdd") + "_" + app_trans_id }, // Format: yyMMdd_xxxx mã giao dịch
                   { "embed_data", JsonConvert.SerializeObject(embed_data) },
                   { "item", JsonConvert.SerializeObject(items) },
                   { "description", "Lazada - Thanh toán đơn hàng #" + app_trans_id },
                   { "bank_code", "" },
                   { "callback_url", "" } // when deploy to add url
               };

                // Generate the MAC
                var data = $"{zaloPaySetting.app_id}|{param["app_trans_id"]}|{param["app_user"]}|{param["amount"]}|{param["app_time"]}|{param["embed_data"]}|{param["item"]}";
                param.Add("mac", HmacHelper.Compute(ZaloPayHMAC.HMACSHA256, zaloPaySetting.key1, data));

                // add to database (app_trans_id, items, description, totalamount)

                // Send the request to ZaloPay
                var result = await HttpHelper.PostFormAsync(zaloPaySetting.create_order_url, param);

                // Return the result to the client
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during payment creation
                return StatusCode(500, new { message = "An error occurred while processing the payment.", details = ex.Message });
            }
        }


        [HttpPost("callback")]
        public IActionResult Post([FromBody] dynamic cbdata)
        {
            var result = new Dictionary<string, object>();

            try
            {
                // Extract data and mac from the incoming request
                var dataStr = Convert.ToString(cbdata["data"]);
                var reqMac = Convert.ToString(cbdata["mac"]);

                // Compute MAC using key2
                var mac = HmacHelper.Compute(ZaloPayHMAC.HMACSHA256, zaloPaySetting.key2, dataStr);
                Console.WriteLine("mac = {0}", mac);

                // Verify if the MAC is valid
                if (!reqMac.Equals(mac))
                {
                    // Invalid callback
                    result["return_code"] = -1;
                    result["return_message"] = "mac not equal";
                }
                else
                {
                    // Valid callback: Payment successful
                    // Update the order status in your system
                    var dataJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataStr);
                    Console.WriteLine("update order's status = success where app_trans_id = {0}", dataJson["app_trans_id"]);

                    //update status database

                    result["return_code"] = 1;
                    result["return_message"] = "success";
                }
            }
            catch (Exception ex)
            {
                // If something went wrong, return an error so that ZaloPay retries the callback
                result["return_code"] = 0; // ZaloPay will retry the callback (up to 3 times)
                result["return_message"] = ex.Message;
            }

            // Return the result to ZaloPay
            return Ok(result);
        }

        [HttpPost("order-status/{app_trans_id}")]
        public async Task<IActionResult> GetOrderStatus(string app_trans_id)
        {
            if (string.IsNullOrEmpty(app_trans_id))
            {
                return BadRequest(new { message = "app_trans_id is required" });
            }

            try
            {
                // Create the request parameters
                var param = new Dictionary<string, string>
                {
                    { "app_id", zaloPaySetting.app_id },
                    { "app_trans_id", app_trans_id }
                };

                // Generate the MAC (Message Authentication Code)
                var data = $"{zaloPaySetting.app_id}|{app_trans_id}|{zaloPaySetting.key1}";
                param.Add("mac", HmacHelper.Compute(ZaloPayHMAC.HMACSHA256, zaloPaySetting.key1, data));

                // Send the request to query the order status
                var result = await HttpHelper.PostFormAsync(zaloPaySetting.query_order_url, param);

                // Return the result as a response
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during the query process
                return StatusCode(500, new { message = "An error occurred while querying the order status.", details = ex.Message });
            }
        }
    }
}
