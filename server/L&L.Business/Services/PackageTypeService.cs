using AutoMapper;
using L_L.Business.Exceptions;
using L_L.Business.Models;
using L_L.Data.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace L_L.Business.Services
{
    public class PackageTypeService
    {
        private readonly UnitOfWorks unitOfWorks;
        private readonly IMapper mapper;

        public PackageTypeService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            this.unitOfWorks = unitOfWorks;
            this.mapper = mapper;
        }

        public async Task<List<PacketTypeModel>> GetAllPackageType()
        {
            var listPackageType = await unitOfWorks.PacketTypeRepository.GetAll().ToListAsync();
            if (listPackageType == null)
            {
                throw new BadRequestException("PackageType not found!");
            }
            return mapper.Map<List<PacketTypeModel>>(listPackageType);
        }
        public async Task<PacketTypeModel> FilterPacketType(decimal weightDecimal, decimal lengthDecimal, decimal widthDecimal, decimal heightDecimal)
        {
            // Retrieve all matching packages
            var packageTypes = await unitOfWorks.PacketTypeRepository
                .FindByCondition(pt => pt.WeightLimit >= weightDecimal &&
                     pt.LengthMin <= lengthDecimal && pt.LengthMax >= lengthDecimal &&
                     pt.WidthMin <= widthDecimal && pt.WidthMax >= widthDecimal &&
                     pt.HeightMin <= heightDecimal && pt.HeightMax >= heightDecimal)
                .ToListAsync(); // Ensure ToListAsync() is called to get all matching records

            if (packageTypes == null || !packageTypes.Any())
            {
                return null;
            }

            // Select the largest package based on a criterion
            var largestPackage = packageTypes
                .OrderBy(pt => pt.WeightLimit) // Adjust this criterion based on what defines "largest"
                .FirstOrDefault();

            return mapper.Map<PacketTypeModel>(largestPackage);
        }

        public async Task<List<PacketTypeModel>> RecommendPacketTypes(decimal weightDecimal, decimal lengthDecimal, decimal widthDecimal, decimal heightDecimal)
        {
            // Tìm tất cả các gói có WeightLimit lớn hơn trọng lượng nhập vào
            var packageTypesGreater = await unitOfWorks.PacketTypeRepository
                .FindByCondition(pt =>
                    pt.WeightLimit > weightDecimal) // Trọng lượng lớn hơn
                .OrderBy(pt => pt.WeightLimit) // Sắp xếp theo WeightLimit để tìm các gói lớn hơn theo thứ tự
                .ToListAsync();

            // Tìm tất cả các gói có WeightLimit nhỏ hơn trọng lượng nhập vào
            var packageTypesLesser = await unitOfWorks.PacketTypeRepository
                .FindByCondition(pt =>
                    pt.WeightLimit < weightDecimal) // Trọng lượng nhỏ hơn
                .OrderByDescending(pt => pt.WeightLimit) // Sắp xếp theo WeightLimit giảm dần để lấy gói gần nhất nhỏ hơn
                .ToListAsync();

            if (!packageTypesGreater.Any() && !packageTypesLesser.Any())
            {
                return null;
            }

            // Lấy gói có WeightLimit lớn hơn gần nhất
            var closestGreaterPackage = mapper.Map<PacketTypeModel>(packageTypesGreater.FirstOrDefault());

            // Lấy gói có WeightLimit lớn hơn một bậc (nếu có)
            var nextGreaterPackage = mapper.Map<PacketTypeModel>(packageTypesGreater.Skip(1).FirstOrDefault());

            // Lấy gói có WeightLimit nhỏ hơn gần nhất
            var closestLesserPackage = mapper.Map<PacketTypeModel>(packageTypesLesser.FirstOrDefault());

            // Tạo danh sách kết quả
            var recommendedPackages = new List<PacketTypeModel>();

            if (closestGreaterPackage != null)
                recommendedPackages.Add(closestGreaterPackage);

            if (nextGreaterPackage != null)
                recommendedPackages.Add(nextGreaterPackage);

            if (closestLesserPackage != null)
                recommendedPackages.Add(closestLesserPackage);

            // Đảm bảo không có trùng lặp trong danh sách
            recommendedPackages = recommendedPackages
                .GroupBy(pt => pt.WeightLimit)
                .Select(g => g.First())
                .ToList();

            return mapper.Map<List<PacketTypeModel>>(recommendedPackages);
        }

        public async Task<List<VehicleTypeModel>> MatchingBaseOnPacketType(PacketTypeModel packageType)
        {
            // Fetch the vehicle types based on the package type's ID and include related entities
            var vehicleTypes = await unitOfWorks.PacketTypeRepository
                .FindByCondition(x => x.PackageTypeId == packageType.PackageTypeId)
                .Include(x => x.VehiclePackageRelations) // Assuming VehiclePackageRelations is a navigation property
                .Select(x => x.VehiclePackageRelations.Select(vpr => vpr.VehicleType)) // Adjust this based on your actual entity relationships
                .ToListAsync();


            var flattenedVehicleTypes = vehicleTypes
                .SelectMany(vpr => vpr)
                .ToList();

            return mapper.Map<List<VehicleTypeModel>>(flattenedVehicleTypes);
        }

        // weight of packet = tấn
        public async Task<decimal> CaculatorService(decimal distance, decimal weight, VehicleTypeModel vehicleType, int orderCount)
        {
            // Constants
            decimal baseDistance = 4; // 4 km đầu tiên
            decimal discountFactor = 1 - (orderCount * 0.1m); // Giảm giá dựa trên số đơn hàng
            if (discountFactor < 0) discountFactor = 0; // Không cho phép giảm giá quá 100%

            // Base cost cho 4 km đầu
            decimal baseCostForFirst4Km = vehicleType.BaseRate;

            // Tính chi phí cho 4 km đầu tiên
            decimal totalCost = (baseCostForFirst4Km / discountFactor);

            // Tính chi phí cho các khoảng cách vượt quá 4 km
            decimal remainingDistance = distance - baseDistance;
            decimal additionalCost = 0;

            if (remainingDistance > 0)
            {
                // Lấy tất cả các ShippingRate phù hợp với VehicleTypeId và bỏ qua những bản ghi có DistanceFrom và DistanceTo bằng 4
                var shippingRates = await unitOfWorks.ShippingRateRepository
                    .FindByCondition(sr => sr.VehicleTypeId == vehicleType.VehicleTypeId &&
                                           !(sr.DistanceFrom == 4 && sr.DistanceTo == 4)) // Điều kiện loại bỏ
                    .OrderBy(sr => sr.DistanceFrom)
                    .ToListAsync();

                foreach (var shippingRate in shippingRates)
                {
                    if (remainingDistance <= 0) break; // Nếu không còn khoảng cách để tính

                    // Kiểm tra nếu DistanceTo là null (nghĩa là cho bất kỳ khoảng cách nào lớn hơn giá trị DistanceFrom)
                    decimal maxDistanceForCurrentRate = shippingRate.DistanceTo.HasValue ? shippingRate.DistanceTo.Value : decimal.MaxValue;

                    // Tính toán khoảng cách áp dụng cho từng mức giá
                    decimal applicableDistance = Math.Min(remainingDistance, maxDistanceForCurrentRate - shippingRate.DistanceFrom + 1);

                    // Tính chi phí cho khoảng cách này
                    additionalCost += applicableDistance * shippingRate.RatePerKM;

                    // Giảm khoảng cách còn lại
                    remainingDistance -= applicableDistance;
                }
            }

            // Tổng chi phí: bao gồm chi phí cho các khoảng cách sau 4km và nhân với trọng lượng
            totalCost += (additionalCost * weight);

            return totalCost;
        }


    }
}
