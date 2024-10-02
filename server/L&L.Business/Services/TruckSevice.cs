using AutoMapper;
using L_L.Business.Commons.Request;
using L_L.Business.Commons.Response;
using L_L.Business.Exceptions;
using L_L.Business.Models;
using L_L.Data.Entities;
using L_L.Data.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace L_L.Business.Services;

public class TruckSevice
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;

    public TruckSevice(UnitOfWorks unitOfWorks, IMapper mapper)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
    }

    public async Task<TruckModel> GetTruckByUserId(string userId)
    {
        var truck = await _unitOfWorks.TruckRepository.FindByCondition(x => x.UserId == int.Parse(userId))
            .FirstOrDefaultAsync();
        if (truck == null)
        {
            throw new BadRequestException("Truck of user not found!");
        }

        return _mapper.Map<TruckModel>(truck);
    }

    public async Task<TruckModel> CreateTruck(TruckRequest truckRequest, int userId)
    {
        var TruckModelNew = new TruckModel()
        {
            TruckName = truckRequest.TruckName,
            Status = truckRequest.Status,
            PlateCode = truckRequest.PlateCode,
            Color = truckRequest.Color,
            Manufacturer = truckRequest.Manufacturer,
            VehicleModel = truckRequest.VehicleModel,
            FrameNumber = truckRequest.FrameNumber,
            EngineNumber = truckRequest.EngineNumber,
            LoadCapacity = truckRequest.LoadCapacity,
            DimensionsLength = truckRequest.DimensionsLength,
            DimensionsWidth = truckRequest.DimensionsWidth,
            DimensionsHeight = truckRequest.DimensionsHeight,
            VehicleTypeId = truckRequest.VehicleTypeId,
            TotalBill = truckRequest.TotalBill,
            UserId = userId
        };
        var TruckCreate =  _mapper.Map<Truck>(TruckModelNew);
        var resultCreate =  await _unitOfWorks.TruckRepository.AddAsync(TruckCreate);
        var result = await _unitOfWorks.TruckRepository.Commit();
        if (result > 0)
        {
            return _mapper.Map<TruckModel>(resultCreate);
        }
        return null;
    }

    public async Task<TruckModel> UpdateTruck(TruckRequest truckRequest, int truckId)
    {
        var truckInfo = await _unitOfWorks.TruckRepository.GetByIdAsync(truckId);
        if (truckInfo == null)
        {
            throw new BadRequestException("Truck not found!");
        }

        truckInfo.TruckName = truckRequest.TruckName;
        truckInfo.Status = truckRequest.Status;
        truckInfo.PlateCode = truckRequest.PlateCode;
        truckInfo.Color = truckRequest.Color;
        truckInfo.Manufacturer = truckRequest.Manufacturer;
        truckInfo.VehicleModel = truckRequest.VehicleModel;
        truckInfo.FrameNumber = truckRequest.FrameNumber;
        truckInfo.EngineNumber = truckRequest.EngineNumber;
        truckInfo.LoadCapacity = truckRequest.LoadCapacity;
        truckInfo.DimensionsLength = truckRequest.DimensionsLength;
        truckInfo.DimensionsWidth = truckRequest.DimensionsWidth;
        truckInfo.DimensionsHeight = truckRequest.DimensionsHeight;
        truckInfo.VehicleTypeId = truckRequest.VehicleTypeId;

        var truckUpdate = _unitOfWorks.TruckRepository.Update(truckInfo);
        var result = await _unitOfWorks.TruckRepository.Commit();
        if (result > 0)
        {
            return _mapper.Map<TruckModel>(truckUpdate);
        }
        return null;
    }

    public async Task<List<DataTruckMile>> GetDistanceForAdmin(int year)
    {
        var DataMonth = new List<DataTruckMile>
        {
            new DataTruckMile { name = "Jan", km = 0 },
            new DataTruckMile { name = "Feb", km = 0 },
            new DataTruckMile { name = "Mar", km = 0 },
            new DataTruckMile { name = "Apr", km = 0 },
            new DataTruckMile { name = "May", km = 0 },
            new DataTruckMile { name = "Jun", km = 0 },
            new DataTruckMile { name = "Jul", km = 0 },
            new DataTruckMile { name = "Aug", km = 0 },
            new DataTruckMile { name = "Sep", km = 0 },
            new DataTruckMile { name = "Oct", km = 0 },
            new DataTruckMile { name = "Nov", km = 0 },
            new DataTruckMile { name = "Dec", km = 0 }
        };

        // Fetch all order details from the repository
        var listOrderDetails =  _unitOfWorks.OrderDetailRepository.GetAll();

        // Iterate through order details and sum the distance based on the month
        foreach (var orderDetail in listOrderDetails)
        {
            if (orderDetail.StartDate.Year == year)
            {
                // Map month to corresponding index (1-based to 0-based)
                var monthIndex = orderDetail.StartDate.Month - 1;

                // Increment the kilometers for the corresponding month
                DataMonth[monthIndex].km += orderDetail.Distance;
            }
        }

        return DataMonth;
    }
    
    public async Task<List<DataTruckStatus>> GetStatusTruckForAdmin(int year)
    {
        var DataMonth = new List<DataTruckStatus>
        {
            new DataTruckStatus { name = "Jan", count = 0 },
            new DataTruckStatus { name = "Feb", count = 0 },
            new DataTruckStatus { name = "Mar", count = 0 },
            new DataTruckStatus { name = "Apr", count = 0 },
            new DataTruckStatus { name = "May", count = 0 },
            new DataTruckStatus { name = "Jun", count = 0 },
            new DataTruckStatus { name = "Jul", count = 0 },
            new DataTruckStatus { name = "Aug", count = 0 },
            new DataTruckStatus { name = "Sep", count = 0 },
            new DataTruckStatus { name = "Oct", count = 0 },
            new DataTruckStatus { name = "Nov", count = 0 },
            new DataTruckStatus { name = "Dec", count = 0 }
        };

        // Fetch all order details from the repository
        var listOrderDetails =  _unitOfWorks.OrderDetailRepository.GetAll();

        // Dictionary to track unique trucks per month
        var trucksPerMonth = new HashSet<string>[12];
        for (int i = 0; i < 12; i++)
        {
            trucksPerMonth[i] = new HashSet<string>();
        }

        // Iterate through order details and count unique trucks for each month
        foreach (var orderDetail in listOrderDetails)
        {
            if (orderDetail.StartDate.Year == year)
            {
                // Map month to corresponding index (1-based to 0-based)
                var monthIndex = orderDetail.StartDate.Month - 1;

                // Add the truck to the corresponding month if not already counted
                trucksPerMonth[monthIndex].Add(orderDetail?.TruckId.ToString());
            }
        }

        // Set the count of active trucks for each month
        for (int i = 0; i < 12; i++)
        {
            DataMonth[i].count = trucksPerMonth[i].Count;  // km now represents the count of active trucks
        }

        return DataMonth;
    }

    public async Task<GetAllTruckPaginationResponse> GetAllTruck(int page)
    {
        const int pageSize = 4; // Set the number of objects per page
        var trukcs = await _unitOfWorks.TruckRepository.GetAll().OrderByDescending(x => x.TruckId).ToListAsync();

        // Calculate total count of trucks
        var totalCount = trukcs.Count();

        // Calculate total pages
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Get the trukcs for the current page
        var pagedTrucks = trukcs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        // Map to TruckModel
        var truckModels = _mapper.Map<List<TruckModel>>(pagedTrucks);

        return new GetAllTruckPaginationResponse()
        {
            data = truckModels,
            pagination = new Pagination
            {
                page = page,
                totalPage = totalPages,
                totalCount = totalCount
            }
        };
    }


}