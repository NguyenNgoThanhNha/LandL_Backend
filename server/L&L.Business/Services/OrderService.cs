using AutoMapper;
using L_L.Business.Commons.Request;
using L_L.Business.Commons.Response;
using L_L.Business.Exceptions;
using L_L.Business.Models;
using L_L.Business.Ultils;
using L_L.Data.Entities;
using L_L.Data.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Net.payOS;
using Net.payOS.Types;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

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

        public async Task<GetAllOrderPaginationResponse> GetAllOrder(int page)
        {
            const int pageSize = 4; // Set the number of objects per page
            var orders = await unitOfWorks.OrderDetailRepository.GetAll().OrderByDescending(x => x.OrderId).ToListAsync();

            // Calculate total count of orders
            var totalCount = orders.Count();

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Get the orders for the current page
            var pagedOrders = orders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Map to OrderModel
            var orderModels = _mapper.Map<List<OrderDetailsModel>>(pagedOrders);

            return new GetAllOrderPaginationResponse()
            {
                data = orderModels,
                pagination = new Pagination
                {
                    page = page,
                    totalPage = totalPages,
                    totalCount = totalCount
                }
            };
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
                    .Include(x => x.DeliveryInfoDetail)
                    .Include(x => x.TruckInfo)
                    .ToListAsync();
                listOrderAdmin.Add(new OrderAdminModel()
                {
                    Order = _mapper.Map<OrderModel>(order),
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

            // Parse the total amount from string to decimal
            decimal totalAmount = decimal.Parse(amount);
            order.TotalAmount = totalAmount;

            // Calculate the 10% charge on the total amount
            decimal charge = totalAmount * 0.10m; // 10% of totalAmount
            order.SystemAmount = charge;

            // Calculate VAT as 8% of the 10% charge
            order.VAT = charge * 0.08m; // 8% of the charge

            // Calculate DriverAmount (TotalAmount - Charge - VAT)
            order.DriverAmount = totalAmount - charge - order.VAT;

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

        public async Task<OrderModel> UpdateStatus(StatusEnums status, OrderModel order)
        {
            order.Status = status.ToString();

            var existingOrder = await unitOfWorks.OrderRepository.GetByIdAsync(order.OrderId);
            if (existingOrder == null)
            {
                throw new BadRequestException("Order not found!");
            }

            _mapper.Map(order, existingOrder);

           var orderUpdate = unitOfWorks.OrderRepository.Update(existingOrder);

          var result =  await unitOfWorks.OrderRepository.Commit();
          if (result > 0)
          {
              return _mapper.Map<OrderModel>(orderUpdate);
          }

          return null;
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

        public async Task<OrderDetailsModel> AddDriverToOrderDetail(AcceptDriverRequest req, int driverId)
        {
            var orderDetail = await unitOfWorks.OrderDetailRepository.GetByIdAsync(int.Parse(req.orderDetailId));
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
            orderDetail.Status = StatusEnums.InProcess.ToString();

            unitOfWorks.TruckRepository.Update(truckOfDriver);
            var orderDetailUpdate =   unitOfWorks.OrderDetailRepository.Update(orderDetail);

            var orderDetailRes = await unitOfWorks.OrderDetailRepository
                .FindByCondition(x => x.OrderDetailId == orderDetailUpdate.OrderDetailId)
                .Include(x => x.OrderInfo)
                .Include(x => x.ProductInfo)
                .Include(x => x.DeliveryInfoDetail)
                .Include(x => x.UserOrder)
                .Include(x => x.TruckInfo)
                .FirstOrDefaultAsync();
            
            var result = await unitOfWorks.OrderRepository.Commit();

            if (result > 0)
            {
                return _mapper.Map<OrderDetailsModel>(orderDetailRes);
            }

            return null;;
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
            // Lấy thông tin tài xế
            var driver = await unitOfWorks.UserRepository.GetByIdAsync(int.Parse(driverId));
            if (driver == null)
            {
                throw new BadRequestException("Driver not found!");
            }

            // Lấy thông tin xe tải của tài xế
            var truckOfDriver = await unitOfWorks.TruckRepository
                .FindByCondition(x => x.UserId == driver.UserId)
                .FirstOrDefaultAsync();
            if (truckOfDriver == null)
            {
                throw new BadRequestException("Truck of driver not found!");
            }

            // Lấy danh sách các đơn hàng đang xử lý
            var listOrderDetail = await unitOfWorks.OrderDetailRepository.GetAll()
                .Include(x => x.ProductInfo)
                .Include(x => x.DeliveryInfoDetail)
                .Where(x => x.Status == StatusEnums.Processing.ToString())
                .ToListAsync();

            var validOrders = new List<OrderDetailsModel>();

            foreach (var orderDetail in listOrderDetail)
            {
                // Kiểm tra tải trọng của xe có đủ để vận chuyển sản phẩm không
                var product = orderDetail.ProductInfo;

                // Parse kích thước sản phẩm từ chuỗi định dạng "lengthxwidthxheight"
                var dimensions = product.TotalDismension.Split('x').Select(decimal.Parse).ToList();
                decimal productVolume = dimensions[0] * dimensions[1] * dimensions[2];
                decimal productWeight = decimal.Parse(product.Weight);

                // So sánh trọng lượng và kích thước sản phẩm với tải trọng của xe
                if (decimal.Parse(truckOfDriver.LoadCapacity) >= productWeight &&
                    truckOfDriver.DimensionsLength >= dimensions[0] &&
                    truckOfDriver.DimensionsWidth >= dimensions[1] &&
                    truckOfDriver.DimensionsHeight >= dimensions[2])
                {
                    // Lấy tọa độ tài xế và điểm lấy hàng
                    double driverLatitude = double.Parse(req.latCurrent);
                    double driverLongitude = double.Parse(req.longCurrent);
                    double pickupLatitude = double.Parse(orderDetail?.DeliveryInfoDetail.LatPickUp);
                    double pickupLongitude = double.Parse(orderDetail?.DeliveryInfoDetail.LongPickUp);

                    // Tính khoảng cách giữa tài xế và điểm giao hàng bằng NetTopologySuite
                    var driverLocation = new Point(driverLongitude, driverLatitude) { SRID = 4326 };
                    var pickupLocation = new Point(pickupLongitude, pickupLatitude) { SRID = 4326 };

                    // Tạo phép biến đổi từ hệ WGS84 sang EPSG:3857 (hệ tọa độ phẳng)
                    var transformFactory = new CoordinateTransformationFactory();
                    var transform = transformFactory.CreateFromCoordinateSystems(
                        GeographicCoordinateSystem.WGS84,
                        ProjectedCoordinateSystem.WebMercator
                    );

                    // Biến đổi tọa độ sang hệ EPSG:3857
                    var driverCoords = new[] { driverLocation.X, driverLocation.Y };
                    var transformedDriverCoords = transform.MathTransform.Transform(driverCoords);
                    var transformedDriverLocation = new Point(transformedDriverCoords[0], transformedDriverCoords[1]) { SRID = 3857 };

                    var pickupCoords = new[] { pickupLocation.X, pickupLocation.Y };
                    var transformedPickupCoords = transform.MathTransform.Transform(pickupCoords);
                    var transformedPickupLocation = new Point(transformedPickupCoords[0], transformedPickupCoords[1]) { SRID = 3857 };

                    // Tính khoảng cách theo mét
                    var distanceToDelivery = transformedDriverLocation.Distance(transformedPickupLocation);

                    if (distanceToDelivery <= 30000)  // đổi sang giới hạn 30km cho tôi
                    {
                        validOrders.Add(new OrderDetailsModel
                        {
                            OrderDetailId = orderDetail.OrderDetailId,
                            PaymentMethod = orderDetail.PaymentMethod,
                            TotalPrice = orderDetail.TotalPrice,
                            Status = orderDetail.Status,
                            OrderId = orderDetail.OrderId,
                            OrderInfo = orderDetail.OrderInfo,
                            ProductId = orderDetail.ProductId,
                            ProductInfo = orderDetail.ProductInfo,
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

            var orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));

            var paymentLinkRequest = new PaymentData(
                orderCode: orderCode, // Generate order code
                amount: amount, // Use the calculated price
                description: $"{orderCode.ToString()}", // Include product description
                items: itemsList, // Pass the items list
                returnUrl: $"{domain}/{req.Request.returnUrl}", // Fix string interpolation
                cancelUrl: $"{domain}/{req.Request.cancelUrl}" // Fix string interpolation
            );

            orderDetail.OrderDetailCode = orderCode;
            unitOfWorks.OrderDetailRepository.Update(orderDetail);
            await unitOfWorks.OrderDetailRepository.Commit();
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
            }
            else if (currentUser.RoleID == 3)
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
            var order = await unitOfWorks.OrderRepository.FindByCondition(x => x.OrderId == int.Parse(orderId))
                .Include(x => x.OrderDriver)
                .FirstOrDefaultAsync();
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
                    .Include(x => x.ProductInfo)
                    .OrderByDescending(x => x.StartDate)
                    .ToListAsync();
            }
            else if (user.RoleID == 3)
            {
                listOrderDetail = await unitOfWorks.OrderDetailRepository
                    .FindByCondition(x => x.OrderId == int.Parse(orderId))
                    .Include(x => x.DeliveryInfoDetail)
                    .Include(x => x.ProductInfo)
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

        public async Task<List<DataOrderCount>> GetOrderInYear(int year)
        {
            var DataCounts = new List<DataOrderCount>
            {
                new DataOrderCount { name = "Jan", count = 0 },
                new DataOrderCount { name = "Feb", count = 0 },
                new DataOrderCount { name = "Mar", count = 0 },
                new DataOrderCount { name = "Apr", count = 0 },
                new DataOrderCount { name = "May", count = 0 },
                new DataOrderCount { name = "Jun", count = 0 },
                new DataOrderCount { name = "Jul", count = 0 },
                new DataOrderCount { name = "Aug", count = 0 },
                new DataOrderCount { name = "Sep", count = 0 },
                new DataOrderCount { name = "Oct", count = 0 },
                new DataOrderCount { name = "Nov", count = 0 },
                new DataOrderCount { name = "Dec", count = 0 }
            };

            var listOrderDetail = unitOfWorks.OrderDetailRepository.GetAll();

            // Duyệt qua các chi tiết đơn hàng và đếm số đơn hàng dựa trên tháng
            foreach (var orderDetail in listOrderDetail)
            {
                if (orderDetail.StartDate.Year == year)
                {
                    // Lấy chỉ số tháng tương ứng (tháng 1 là index 0)
                    var monthIndex = orderDetail.StartDate.Month - 1;

                    // Tăng số lượng đơn hàng của tháng tương ứng
                    DataCounts[monthIndex].count++;
                }
            }

            return DataCounts;
        }

        public async Task<List<DataAmount>> GetOrderAmountInYear(int year)
        {
            // Predefined list of months with initial values for total, system, and VAT amounts.
            var dataCounts = new List<DataAmount>
            {
                new DataAmount { name = "Jan", total = 0, system = 0, vat = 0 },
                new DataAmount { name = "Feb", total = 0, system = 0, vat = 0 },
                new DataAmount { name = "Mar", total = 0, system = 0, vat = 0 },
                new DataAmount { name = "Apr", total = 0, system = 0, vat = 0 },
                new DataAmount { name = "May", total = 0, system = 0, vat = 0 },
                new DataAmount { name = "Jun", total = 0, system = 0, vat = 0 },
                new DataAmount { name = "Jul", total = 0, system = 0, vat = 0 },
                new DataAmount { name = "Aug", total = 0, system = 0, vat = 0 },
                new DataAmount { name = "Sep", total = 0, system = 0, vat = 0 },
                new DataAmount { name = "Oct", total = 0, system = 0, vat = 0 },
                new DataAmount { name = "Nov", total = 0, system = 0, vat = 0 },
                new DataAmount { name = "Dec", total = 0, system = 0, vat = 0 }
            };

            // Fetch only the orders from the specified year, projecting only relevant fields
            var listOrder = await unitOfWorks.OrderRepository
                .GetAll()
                .Where(o => o.OrderDate.Year == year)  // Filter by the year
                .Select(o => new
                {
                    o.OrderDate,
                    o.TotalAmount,
                    o.SystemAmount,
                    o.VAT
                })
                .ToListAsync();  // Asynchronously fetch orders from the database

            // Iterate through the filtered orders
            foreach (var order in listOrder)
            {
                // Calculate the month index (January is 0)
                var monthIndex = order.OrderDate.Month - 1;

                // Accumulate total, system amount, and VAT for the respective month
                dataCounts[monthIndex].total += order.TotalAmount ?? 0;
                dataCounts[monthIndex].system += order.SystemAmount ?? 0;
                dataCounts[monthIndex].vat += order.VAT ?? 0;
            }

            return dataCounts;
        }

    }
}
