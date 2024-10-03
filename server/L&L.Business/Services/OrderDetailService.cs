using AutoMapper;
using L_L.Business.Commons.Request;
using L_L.Business.Exceptions;
using L_L.Business.Models;
using L_L.Business.Ultils;
using L_L.Data.Entities;
using L_L.Data.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace L_L.Business.Services
{
    public class OrderDetailService
    {
        private readonly UnitOfWorks unitOfWorks;
        private readonly IMapper mapper;

        public OrderDetailService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            this.unitOfWorks = unitOfWorks;
            this.mapper = mapper;
        }
        
        public async Task<OrderDetailsModel> GetOrderDetail(int id)
        {
            var order = await unitOfWorks.OrderDetailRepository.GetByIdAsync(id);
            if (order != null)
            {
                return mapper.Map<OrderDetailsModel>(order);
            }
            return null;
        }

        public async Task<List<OrderDetailsModel>> GetAll()
        {
            return mapper.Map<List<OrderDetailsModel>>(unitOfWorks.OrderDetailRepository.GetAll().ToListAsync());
        }

        public async Task<OrderDetailsModel> CreateOrderDetail(int orderId, CreateOrderRequest req, int userId)
        {
            var order = await unitOfWorks.OrderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new BadRequestException("Order not found!");
            }

            var userSender = await unitOfWorks.UserRepository.FindByCondition(x => x.Email == req.Email).FirstOrDefaultAsync();
            if (userSender == null)
            {
                throw new BadRequestException("User Sender not found!");
            }

            // create product
            var product = await unitOfWorks.ProductRepository.AddAsync(new Data.Entities.Product()
            {
                TotalDismension = $"{req.Width} x {req.Height} x {req.Length}",
                Weight = req.Weight.ToString(),
                TypeProduct = req.Type,
                SenderId = userId,
                Quantity = 1
            });
            var resultProduct = await unitOfWorks.ProductRepository.Commit();

            // create delivery info
            var delivery = await unitOfWorks.DeliveryInfoRepository.AddAsync(new Data.Entities.DeliveryInfo()
            {
                PickUpLocation = req.From,
                DeliveryLocaTion = req.To,
                LongPickUp = req.longFrom,
                LatPickUp = req.latFrom,
                LongDelivery = req.longTo,
                LatDelivery = req.latTo,
                OrderDate = order.OrderDate,
                RecieveDate = req.PickupTime,
                SenderName = userSender?.UserName,
                SenderPhone = userSender?.PhoneNumber
            });
            var resultDeliveryInfo = await unitOfWorks.DeliveryInfoRepository.Commit();

            // create order detail
            var orderDetail = await unitOfWorks.OrderDetailRepository.AddAsync(new Data.Entities.OrderDetails()
            {
                Status = StatusEnums.Processing.ToString(),
                ProductId = product.ProductId,
                DeliveryInfoId = delivery.DeliveryInfoId,
                SenderId = userSender?.UserId,
                OrderId = order.OrderId,
                TotalPrice = req.TotalAmount,
                VehicleTypeId = req.VehicleTypeId,
                StartDate = order.OrderDate,
                Distance = req.Distance
            });

            var result = await unitOfWorks.OrderDetailRepository.Commit();

            // create service cost
            var listServiceCost = new List<ServiceCost>();
            foreach (var cost in req.listCost)
            {
                var serviceCost = new ServiceCost()
                {
                    VehicleTypeId = cost.Key,
                    Amount = cost.Value.ToString(),
                    OrderDetailId = orderDetail.OrderDetailId
                };
                listServiceCost.Add(serviceCost);
            }

            await unitOfWorks.ServiceCostRepository.AddRangeAsync(listServiceCost);

            if (orderDetail != null && result > 0 && resultProduct > 0 && resultDeliveryInfo > 0)
            {
                return mapper.Map<OrderDetailsModel>(orderDetail);
            }

            return null;
        }

        public async Task<OrderDetailsModel> GetOrderByOrderDetailCode(string orderDetailCode)
        {
            // Ensure to handle the parsing correctly to avoid exceptions
            if (!int.TryParse(orderDetailCode, out int code))
            {
                throw new BadRequestException("Invalid order detail code format!");
            }

            var orderDetail = await unitOfWorks.OrderDetailRepository
                .FindByCondition(x => x.OrderDetailCode == code)
                .FirstOrDefaultAsync();

            if (orderDetail == null)
            {
                throw new BadRequestException("Order detail not found!");
            }

            return mapper.Map<OrderDetailsModel>(orderDetail);
        }

        public async Task<bool> UpdateOrderAfterPayment(OrderDetailsModel order)
        {
            var orderUpdate = mapper.Map<OrderDetails>(order);
            unitOfWorks.OrderDetailRepository.Update(orderUpdate);
            return await unitOfWorks.OrderDetailRepository.Commit() > 0;
        }

        public async Task<OrderDetailsModel> UpdateStatusOrderDetail(StatusEnums statusEnums, OrderDetailsModel orderDetailsModel)
        {
            orderDetailsModel.Status = statusEnums.ToString();

            var existingOrder = await unitOfWorks.OrderDetailRepository.GetByIdAsync(orderDetailsModel.OrderDetailId);
            if (existingOrder == null)
            {
                throw new BadRequestException("Order detail not found!");
            }

            mapper.Map(orderDetailsModel, existingOrder);

            var orderDetailUpdate =  unitOfWorks.OrderDetailRepository.Update(existingOrder);

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
               return mapper.Map<OrderDetailsModel>(orderDetailRes);
           }
            return null;
        }

        public async Task<OrderDetailsModel> GetOrderDetailByOrderDetailId(int id)
        {
            var orderDetail = await unitOfWorks.OrderDetailRepository.FindByCondition(x => x.OrderDetailId == id)
                .Include(x => x.OrderInfo)
                .Include(x => x.ProductInfo)
                .Include(x => x.DeliveryInfoDetail)
                .Include(x => x.TruckInfo)
                .Include(x => x.UserOrder)
                .FirstOrDefaultAsync();
            return mapper.Map<OrderDetailsModel>(orderDetail);
        }
    }
}
