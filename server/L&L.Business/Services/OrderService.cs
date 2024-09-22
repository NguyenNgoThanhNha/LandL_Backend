using AutoMapper;
using L_L.Business.Commons.Request;
using L_L.Business.Exceptions;
using L_L.Business.Models;
using L_L.Business.Ultils;
using L_L.Data.Entities;
using L_L.Data.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Net.payOS;
using Net.payOS.Types;
using NetTopologySuite.Geometries;

namespace L_L.Business.Services
{
    public class OrderService
    {
        private readonly UnitOfWorks unitOfWorks;
        private readonly IMapper _mapper;
        private readonly PayOSSetting _payOsSetting;
        private readonly CloudService _cloudService;

        public OrderService(UnitOfWorks unitOfWorks, IMapper mapper, PayOSSetting payOsSetting, CloudService cloudService)
        {
            this.unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _payOsSetting = payOsSetting;
            _cloudService = cloudService;
        }

        public async Task<List<OrderModel>> GetAll()
        {
            return _mapper.Map<List<OrderModel>>(await unitOfWorks.OrderRepository.GetAll().OrderByDescending(x => x.OrderDate).ToListAsync());
        }
        
        public async Task<List<OrderAdminModel>> GetAllOrderForAdmin()
        {
            var listOrderAdmin = new List<OrderAdminModel>();
            var listOrder = await unitOfWorks.OrderRepository.GetAll().OrderByDescending(x => x.OrderDate)
                .ToListAsync();
            foreach (var order in listOrder)
            {
                var listOrderDetail = await unitOfWorks.OrderDetailRepository
                    .FindByCondition(x => x.OrderId == order.OrderId)
                    .Include(x => x.ProductInfo)
                    .Include(x =>x.DeliveryInfoDetail)
                    .Include(x=> x.TruckInfo)
                    .ToListAsync();
                listOrderAdmin.Add(new OrderAdminModel()
                {
                    Order =  _mapper.Map<OrderModel>(order),
                    OrderDetails = _mapper.Map<List<OrderDetailsModel>>(listOrderDetail)
                });
            }

            if (listOrderAdmin == null)
            {
                throw new BadRequestException("Now not have any order!");
            }
            return listOrderAdmin;
        }

        public async Task<OrderModel> CreateOrder(string amount, DateTime pickUpTime)
        {
            var order = new OrderModel();
            order.Status = StatusEnums.Processing.ToString();
            order.TotalAmount = decimal.Parse(amount);
            order.OrderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
            order.OrderCount = 1;
            order.OrderDate = pickUpTime;
            var orderCreate = await unitOfWorks.OrderRepository.AddAsync(_mapper.Map<Order>(order));
            var result = await unitOfWorks.OrderRepository.Commit();
            if (result > 0)
            {
                return _mapper.Map<OrderModel>(orderCreate);
            }
            return null;
        }

        public async Task<OrderModel> GetOrder(int id)
        {
            var order = await unitOfWorks.OrderRepository.GetByIdAsync(id);
            if (order != null)
            {
                return _mapper.Map<OrderModel>(order);
            }
            return null;
        }

        public async Task<bool> UpdateStatus(StatusEnums status, OrderModel order)
        {
            order.Status = status.ToString();

            var existingOrder = await unitOfWorks.OrderRepository.GetByIdAsync(order.OrderId);
            if (existingOrder == null)
            {
                return false;
            }

            _mapper.Map(order, existingOrder);

            unitOfWorks.OrderRepository.Update(existingOrder);

            await unitOfWorks.OrderRepository.Commit();
            return true;
        }

        public async Task<List<OrderDetailsModel>> PublicOrder()
        {
            var listOrderDetail = await unitOfWorks.OrderDetailRepository.GetAll()
                .Include(o => o.DeliveryInfoDetail)  // Include bảng DeliveryInfo
                /*                .Include(o => o.TruckInfo)           // Include bảng Truck*/
                .Include(o => o.ProductInfo)         // Include bảng Product
                .Include(o => o.OrderInfo)           // Include bảng Order
                /*                .Include(o => o.UserOrder)           // Include bảng User*/
                .Where(o => o.Status == StatusEnums.Processing.ToString())
                .ToListAsync();
            return _mapper.Map<List<OrderDetailsModel>>(listOrderDetail);
        }

        public async Task<bool> AddDriverToOrderDetail(AcceptDriverRequest req, int driverId)
        {
            var orderDetail = await unitOfWorks.OrderDetailRepository
                .FindByCondition(x=> x.OrderDetailId == int.Parse(req.orderDetailId))
                .Include(x =>x.ProductInfo)
                .FirstOrDefaultAsync();
            if (orderDetail == null)
            {
                throw new BadRequestException("Order detail not found!");
            }
            
            var order = await unitOfWorks.OrderRepository.GetByIdAsync((int)orderDetail.OrderId);
            if (order == null || order.OrderId != orderDetail.OrderId)
            {
                throw new BadRequestException("Order detail of order are invalid!");
            }
            
            order.DriverId = driverId;
            unitOfWorks.OrderRepository.Update(order);
            
            var truckOfDriver = await unitOfWorks.TruckRepository.FindByCondition(x => x.UserId == order.DriverId).FirstOrDefaultAsync();
            if (truckOfDriver == null)
            {
                throw new BadRequestException("Truck of driver not found!");
            }
            
            var product = orderDetail.ProductInfo;
            var dimensions = product.TotalDismension.Split('x').Select(decimal.Parse).ToList();
            decimal productLength = dimensions[0];
            decimal productWidth = dimensions[1];
            decimal productHeight = dimensions[2];
            
            truckOfDriver.DimensionsLength -= productLength;
            truckOfDriver.DimensionsWidth -= productWidth;
            truckOfDriver.DimensionsHeight -= productHeight;
            
            // add truck to order detail
            orderDetail.TruckId = truckOfDriver.TruckId;
            
            unitOfWorks.TruckRepository.Update(truckOfDriver);
            unitOfWorks.OrderDetailRepository.Update(orderDetail);
            
            var result = await unitOfWorks.OrderRepository.Commit();
            
            return result > 0 ;
        }


        public async Task<ProductsModel> UpdateProductInOrderDetail(UpdateProductInfoRequest req)
        {
            var orderDetail = await unitOfWorks.OrderDetailRepository
                .FindByCondition(x => x.OrderDetailId == int.Parse(req.orderDetailId))
                .Include(x => x.ProductInfo)
                .FirstOrDefaultAsync();
            if (orderDetail == null)
            {
                throw new BadRequestException("Order detail of product is not found");
            }

            var productInfo = orderDetail.ProductInfo;
            if (orderDetail.ProductId != productInfo.ProductId)
            {
                throw new BadRequestException("Order detail have not product with info product provide!");
            }
            productInfo.ProductName = req.ProductName;
            productInfo.ProductDescription = req.ProductDescription;
            if (req.Image != null)
            {
                var uploadResult = await _cloudService.UploadImageAsync(req.Image);

                if (uploadResult.Error == null)
                {
                    productInfo.Image = uploadResult.SecureUrl.ToString();
                }
                else
                {
                    throw new BadRequestException("Error in update image of product");
                }
            }
            var resultUpdate = unitOfWorks.ProductRepository.Update(productInfo);
            var result = await unitOfWorks.ProductRepository.Commit();
            if (result > 0)
            {
                return _mapper.Map<ProductsModel>(resultUpdate);
            }
            return null;
        }

        public async Task<DeliveryInfoModel> UpdateDiveryInOrderDetail(UpdateDeliveryInfoRequest req)
        {
            var orderDetail = await unitOfWorks.OrderDetailRepository.GetByIdAsync(int.Parse(req.orderDetailId));
            if (orderDetail == null)
            {
                throw new BadRequestException("Order detail of product is not found");
            }
            var deiveryUpdate = await unitOfWorks.DeliveryInfoRepository.GetByIdAsync(int.Parse(req.deliveryInfoId));
            deiveryUpdate.SenderName = req.senderName;
            deiveryUpdate.SenderPhone = req.senderPhone;
            deiveryUpdate.ReceiverName = req.receiverName;
            deiveryUpdate.ReceiverPhone = req.receiverPhone;
            var resultUpdate = unitOfWorks.DeliveryInfoRepository.Update(deiveryUpdate);
            var result = await unitOfWorks.ProductRepository.Commit();
            if (result > 0)
            {
                return _mapper.Map<DeliveryInfoModel>(resultUpdate);
            }
            return null;
        }

        public async Task<List<OrderDetailsModel>> GetOrderForDriver(string driverId, GetOrderOfDriverRequest req)
        {
            // Get the driver details
            var driver = await unitOfWorks.UserRepository.GetByIdAsync(int.Parse(driverId));
            if (driver == null)
            {
                throw new BadRequestException("Driver not found!");
            }

            // Get the truck details associated with the driver
            var truckOfDriver = await unitOfWorks.TruckRepository.FindByCondition(x => x.UserId == driver.UserId).FirstOrDefaultAsync();
            if (truckOfDriver == null)
            {
                throw new BadRequestException("Truck of driver not found!");
            }

            // Fetch list of order details that are still processing
            var listOrderDetail = await unitOfWorks.OrderDetailRepository.GetAll()
                .Include(x => x.ProductInfo)
                .Include(x => x.DeliveryInfoDetail)
                .Where(x => x.Status == StatusEnums.Processing.ToString())
                .ToListAsync();

            var validOrders = new List<OrderDetailsModel>();

            foreach (var orderDetail in listOrderDetail)
            {
                // Check if truck capacity allows carrying the product
                var product = orderDetail.ProductInfo;

                // Parse the product's dimensions (assuming TotalDimension is in the format "length*width*height")
                var dimensions = product.TotalDismension.Split('x').Select(decimal.Parse).ToList();
                decimal productVolume = dimensions[0] * dimensions[1] * dimensions[2];
                decimal productWeight = decimal.Parse(product.Weight);

                // Compare product's volume and weight with truck's capacity
                if (decimal.Parse(truckOfDriver.LoadCapacity) >= productWeight &&
                    truckOfDriver.DimensionsLength >= dimensions[0] &&
                    truckOfDriver.DimensionsWidth >= dimensions[1] &&
                    truckOfDriver.DimensionsHeight >= dimensions[2])
                {
                    // Now check proximity to driver
                    double driverLatitude = double.Parse(req.latCurrent);
                    double driverLongitude = double.Parse(req.longCurrent);
                    double deliveryLatitude = double.Parse(orderDetail?.DeliveryInfoDetail.LatDelivery);
                    double deliveryLongitude = double.Parse(orderDetail?.DeliveryInfoDetail.LongDelivery);

                    // Use NetTopologySuite for distance calculation
                    var driverLocation = new Coordinate(driverLongitude, driverLatitude);
                    var deliveryLocation = new Coordinate(deliveryLongitude, deliveryLatitude);
                    var pointA = new Point(driverLocation);
                    var pointB = new Point(deliveryLocation);
                    var distanceToDelivery = pointA.Distance(pointB) * 1000; // Convert from degrees to meters

                    if (distanceToDelivery <= 5000) // For example, orders within a 5km radius
                    {
                        validOrders.Add(new OrderDetailsModel
                        {
                            OrderDetailId = orderDetail.OrderDetailId,
                            Quantity = orderDetail.Quantity,
                            PaymentMethod = orderDetail.PaymentMethod,
                            UnitPrice = orderDetail.UnitPrice,
                            TotalPrice = orderDetail.TotalPrice,
                            Status = orderDetail.Status,
                            /*                            VehicleTypeId = orderDetail.VehicleTypeId,*/
                            /*                            SenderId = orderDetail.SenderId,
                                                        UserOrder = orderDetail.UserOrder,*/
                            OrderId = orderDetail.OrderId,
                            OrderInfo = orderDetail.OrderInfo,
                            ProductId = orderDetail.ProductId,
                            ProductInfo = orderDetail.ProductInfo,
                            /*                            TruckId = truckOfDriver.TruckId,  // Assign the driver's truck
                                                        TruckInfo = truckOfDriver,        // Include truck info*/
                            DeliveryInfoId = orderDetail.DeliveryInfoId,
                            DeliveryInfoDetail = orderDetail.DeliveryInfoDetail
                        });
                    }
                }
            }

            return validOrders;
        }

        public async Task<string> ConfirmOrderDetail(ConfirmOrderRequest req)
        {
            var orderDetail = await unitOfWorks.OrderDetailRepository
                .FindByCondition(x => x.OrderDetailId == req.orderDetailId)
                .Include(x => x.ProductInfo)
                .Include(x => x.DeliveryInfoDetail)
                .Include(x => x.OrderInfo)
                .FirstOrDefaultAsync();

            if (orderDetail == null)
            {
                throw new BadRequestException("Order Detail not found!");
            }

            // Execute PayOS
            var payOS = new PayOS(_payOsSetting.ClientId, _payOsSetting.ApiKey, _payOsSetting.ChecksumKey);
            var domain = _payOsSetting.Domain;

            var product = await unitOfWorks.ProductRepository.GetByIdAsync((int)orderDetail.ProductId);
            var amount = Convert.ToInt32(req.totalAmount);
            var price = Convert.ToInt32(orderDetail.TotalPrice); // Rounding to the nearest integer

            if (product == null)
            {
                throw new BadRequestException("Product cannot be found!");
            }

            // Create the items list with proper syntax
            var itemsList = new List<ItemData>
            {
                new ItemData($"{product.ProductName}", 1, price)
            };

            var paymentLinkRequest = new PaymentData(
                orderCode: int.Parse(DateTimeOffset.Now.ToString("ffffff")), // Generate order code
                amount: amount, // Use the calculated price
                description: "Payment for service", // Include product description
                items: itemsList, // Pass the items list
                returnUrl: $"{domain}/{req.Request.returnUrl}", // Fix string interpolation
                cancelUrl: $"{domain}/{req.Request.cancelUrl}" // Fix string interpolation
            );

            try
            {
                var response = await payOS.createPaymentLink(paymentLinkRequest);
                return response.checkoutUrl;
            }
            catch (Exception ex)
            {
                // Handle exception appropriately (log it, rethrow it, etc.)
                throw new Exception("Failed to create payment link.", ex);
            }
        }




        public async Task<List<OrderModel>> GetOrderByUserId(string userId)
        {
            var listOrderResult = new List<OrderModel>();
            var currentUser = await unitOfWorks.UserRepository.GetByIdAsync(int.Parse(userId));
            // customer
            if (currentUser.RoleID == 2)
            {
                var orderDetails = await unitOfWorks.OrderDetailRepository.FindByCondition(x => x.SenderId == currentUser.UserId).ToListAsync();
                if (orderDetails.Any() && orderDetails != null)
                {
                    foreach (var orderDetail in orderDetails)
                    {
                        var order = await unitOfWorks.OrderRepository.GetByIdAsync((int)orderDetail.OrderId);
                        listOrderResult.Add(_mapper.Map<OrderModel>(order));
                    }

                    return listOrderResult;
                }
                else
                {
                    throw new BadRequestException("Order of user not found!");
                }
            } else if (currentUser.RoleID == 3)
            {
                var orders = await unitOfWorks.OrderRepository.FindByCondition(x => x.DriverId == currentUser.UserId)
                    .ToListAsync();
                if (orders == null || !orders.Any())
                {
                    throw new BadRequestException("Order of user not found!");
                }
                return _mapper.Map<List<OrderModel>>(orders);
            }
            return null;
        }

        public async Task<OrderModel> GetOrderByOrderId(string orderId)
        {
            var order = await unitOfWorks.OrderRepository.GetByIdAsync(int.Parse(orderId));
            if (order == null)
            {
                throw new BadRequestException("Order not found!");
            }

            return _mapper.Map<OrderModel>(order);
        }

        public async Task<List<OrderDetailsModel>> GetOrderDetailByOrderId(string orderId, UserModel user)
        {
            var listOrderDetail = new List<OrderDetails>();
            if (user.RoleID == 2)
            {
                listOrderDetail = await unitOfWorks.OrderDetailRepository
                    .FindByCondition(x => x.OrderId == int.Parse(orderId) && x.SenderId == user.UserId)
                    .Include(x => x.DeliveryInfoDetail)
                    .Include(x =>x.ProductInfo)
                    .OrderByDescending(x => x.StartDate)
                    .ToListAsync();
            }else if (user.RoleID == 3)
            {
                listOrderDetail = await unitOfWorks.OrderDetailRepository
                    .FindByCondition(x => x.OrderId == int.Parse(orderId))
                    .Include(x => x.DeliveryInfoDetail)
                    .Include(x =>x.ProductInfo)
                    .OrderByDescending(x => x.StartDate)
                    .ToListAsync();
                if (!listOrderDetail.Any() || listOrderDetail == null)
                {
                    throw new BadRequestException("Order detail of Order not found!");
                }
            }

            if (listOrderDetail != null)
            {
                return _mapper.Map<List<OrderDetailsModel>>(listOrderDetail);
            }
            return null;
        }

    }
}
