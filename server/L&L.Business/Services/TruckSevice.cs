using AutoMapper;
using L_L.Business.Exceptions;
using L_L.Business.Models;
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
}