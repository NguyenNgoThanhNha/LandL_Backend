using AutoMapper;
using L_L.Business.Commons.Request;
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
}