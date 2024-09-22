using L_L.Business.Commons;
using L_L.Business.Commons.Request;
using L_L.Business.Commons.Response;
using L_L.Business.Models;
using L_L.Business.Services;
using L_L.Business.Ultils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L_L.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderService orderService;
        private readonly OrderDetailService orderDetailService;
        private readonly UserService userService;

        public OrderController(OrderService orderService, OrderDetailService orderDetailService, UserService userService)
        {
            this.orderService = orderService;
            this.orderDetailService = orderDetailService;
            this.userService = userService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllOrder()
        {
            var listOrder = await orderService.GetAll();
            return Ok(ApiResult<OrderListResponse>.Succeed(new OrderListResponse()
            {
                data = listOrder
            }));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetOrdesAdmin")]
        public async Task<IActionResult> GetOrderForAdmin()
        {
            var listOrderAdmin = await orderService.GetAllOrderForAdmin();
            if (listOrderAdmin == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Now have not any order!"
                }));
            }
            return Ok(ApiResult<ListOrderAdminResponse>.Succeed(new ListOrderAdminResponse()
            {
                data = listOrderAdmin
            }));
        }

        [HttpGet("GetOrderByUserId")]
        public async Task<IActionResult> GetOrderByUserId()
        {
            // Lấy token từ header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return Unauthorized(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Authorization header is missing."
                }));
            }
            
            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            var currentUser = await userService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Driver not found."
                }));
            }

            var listOrder = await orderService.GetOrderByUserId(currentUser.UserId.ToString());
            if (listOrder == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Order of user not found!"
                }));
            }
            return Ok(ApiResult<OrderListResponse>.Succeed(new OrderListResponse()
            {
                data = listOrder
            }));
        }

        [HttpGet("GetOrderByOrderId")]
        public async Task<IActionResult> GetOrderByOrderId([FromQuery] string orderId)
        {
            // Lấy token từ header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return Unauthorized(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Authorization header is missing."
                }));
            }

            var order = await orderService.GetOrderByOrderId(orderId);
            if (order == null) 
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Order not found!"
                }));
            }
            
            return Ok(ApiResult<OrderResponse>.Succeed(new OrderResponse()
            {
                data = order
            }));
        }
        [Authorize(Roles = "Customer, Driver")]
        [HttpGet("GetOrderDetailByOrderId")]
        public async Task<IActionResult> GetOrderDetailByOrderId([FromQuery] string orderId)
        {
            // Lấy token từ header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return Unauthorized(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Authorization header is missing."
                }));
            }
            
            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            var currentUser = await userService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Driver not found."
                }));
            }

            var listOrderDetail = new List<OrderDetailsModel>();

            // customer
            if (currentUser.RoleID == 2)
            {
                listOrderDetail = await orderService.GetOrderDetailByOrderId(orderId, currentUser);
            }else if (currentUser.RoleID == 3)
            {
                listOrderDetail = await orderService.GetOrderDetailByOrderId(orderId, currentUser);
            }
            
            if (listOrderDetail == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = $"Order detail of order id: {orderId} not found!"
                }));
            }
            
            return Ok(ApiResult<OrderDetailResponse>.Succeed(new OrderDetailResponse()
            {
                data = listOrderDetail
            }));
        }
        
        [Authorize(Roles = "Customer")] 
        [HttpPost("Create_Order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            // Lấy token từ header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return Unauthorized(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Authorization header is missing."
                }));
            }
            
            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            var currentUser = await userService.GetUserInToken(tokenValue);
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            // create order
            var orderCreate = await orderService.CreateOrder(request.TotalAmount.ToString(), request.PickupTime);
            if (orderCreate == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "Error in create order"
                }));
            }

            // create order detail
            var orderDetailCreate = await orderDetailService.CreateOrderDetail(orderCreate.OrderId, request, currentUser.UserId);
            if (orderDetailCreate == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "Error in create order detail"
                }));
            }

            return Ok(ApiResult<ResponseMessage>.Succeed(new ResponseMessage()
            {
                message = "Create order success!",
                data = orderCreate.OrderId.ToString()
            }));
        }

        [HttpPut("Update-Status/{id}")]
        public async Task<IActionResult> UpdateStatusOrder([FromRoute] string id, [FromBody] StatusEnums status)
        {
            var order = await orderService.GetOrder(int.Parse(id));
            if (order == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "Order not found!"
                }));
            }

            var result = await orderService.UpdateStatus(status, order);

            if (result)
            {
                return Ok(ApiResult<ResponseMessage>.Succeed(new ResponseMessage()
                {
                    message = "Update order status success!"
                }));
            }

            return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
            {
                message = "Update order status error!"
            }));
        }

        [HttpGet("public-order")]
        public async Task<IActionResult> PublicOrder()
        {
            var listOrderDetail = await orderService.PublicOrder();
            return Ok(ApiResult<List<OrderDetailsModel>>.Succeed(listOrderDetail));
        }

        [Authorize(Roles = "Driver")]
        [HttpPost("accept-driver")]
        public async Task<IActionResult> AddDriverToOrderDetail([FromBody] AcceptDriverRequest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }
            
            // Lấy token từ header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return Unauthorized(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Authorization header is missing."
                }));
            }
            
            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            var currentUser = await userService.GetUserInToken(tokenValue);

            var result = await orderService.AddDriverToOrderDetail(req, currentUser.UserId);
            if (!result)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "Error in accept order!"
                }));
            }
            return Ok(ApiResult<ResponseMessage>.Succeed(new ResponseMessage()
            {
                message = "Accept order success"
            }));
        }

        [HttpPatch("UpdateProductInfo")]
        public async Task<IActionResult> UpdateOrderDetail([FromForm] UpdateProductInfoRequest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }
            var updateProductResult = await orderService.UpdateProductInOrderDetail(req);
            if (updateProductResult == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = $"Error in update product of Order Detail with ${req.orderDetailId}"
                }));
            }
            return Ok(ApiResult<ResponseMessage>.Succeed(new ResponseMessage()
            {
                message = "Update product in order detail success"
            }));
        }

        [HttpPatch("UpdateDeliveryInfo")]
        public async Task<IActionResult> UpdateDeliveryInfo([FromBody] UpdateDeliveryInfoRequest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }
            var updateDeliveryResult = await orderService.UpdateDiveryInOrderDetail(req);
            if (updateDeliveryResult == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = $"Error in update delivery of Order Detail with ${req.orderDetailId}"
                }));
            }
            return Ok(ApiResult<ResponseMessage>.Succeed(new ResponseMessage()
            {
                message = "Update delivery info in order detail success"
            }));
        }

        [Authorize(Roles = "Driver")]
        [HttpPost("GetOrderDriver")]
        public async Task<IActionResult> GetOrderOfDriver([FromBody] GetOrderOfDriverRequest req)
        {
            // Lấy token từ header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return Unauthorized(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Authorization header is missing."
                }));
            }

            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            var currentUser = await userService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ResponseMessage>.Error(new ResponseMessage
                {
                    message = "Driver not found."
                }));
            }

            // Lấy danh sách đơn hàng cho tài xế
            var listOrder = await orderService.GetOrderForDriver(currentUser.UserId.ToString(), req);

            if (listOrder == null || !listOrder.Any())
            {
                return Ok(ApiResult<ResponseMessage>.Succeed(new ResponseMessage()
                {
                    message = "Currently, cannot find order suitable."
                }));
            }

            return Ok(ApiResult<List<OrderDetailsModel>>.Succeed(listOrder));
        }

        [HttpPost("ConfirmOrder")]
        public async Task<IActionResult> ConfirmOrderDetail([FromBody] ConfirmOrderRequest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var urlPayemnt = await orderService.ConfirmOrderDetail(req);

            if (urlPayemnt == null)
            {
                return Ok(ApiResult<ResponseMessage>.Error(new ResponseMessage()
                {
                    message = "Error in create payment"
                }));
            }
            return Ok(ApiResult<ResponseMessage>.Succeed(new ResponseMessage()
            {
                message =urlPayemnt
            }));
        }
    }


}
