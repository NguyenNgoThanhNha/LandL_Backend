using AutoMapper;
using L_L.Business.Commons.Request;
using L_L.Business.Exceptions;
using L_L.Business.Models;
using L_L.Data.Entities;
using L_L.Data.Repositories;
using L_L.Data.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace L_L.Business.Services;

public class GuessService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;

    public GuessService(UnitOfWorks unitOfWorks, IMapper mapper)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
    }

    public async Task<bool> CreateNewGuess(string email)
    {
        var guess = await _unitOfWorks.GuessRepository
            .FindByCondition(x => x.email.ToLower().Equals(email.ToLower()))
            .FirstOrDefaultAsync();

        if (guess != null)
        {
            guess.UpdatedAt = DateTime.Now;
            _unitOfWorks.GuessRepository.Update(guess);
        }
        else
        {
            var guessModel = new GuessModel()
            {
                email = email.ToLower(),
                status = "Active",
                CreatedAt = DateTime.Now
            };
            var guessCreate = _mapper.Map<Guess>(guessModel);
            await _unitOfWorks.GuessRepository.AddAsync(guessCreate);
        }

        var result = await _unitOfWorks.GuessRepository.Commit();
        if (result > 0)
        {
            return true;
        }

        throw new BadRequestException("Error in processing guess info");
    }
    
    public async Task<bool> CollectInfoCustomer(CollectInfoCustomerRequest request)
    {
        var guessInfo = await _unitOfWorks.GuessRepository
            .FindByCondition(x => x.email.ToLower() == request.email.ToLower())
            .FirstOrDefaultAsync();
        if (guessInfo != null)
        {
            guessInfo.phone = request.phone != null ? request.phone : guessInfo.phone;
            guessInfo.age = request.age != 0 ? request.age : guessInfo.age;
            guessInfo.priorityAddress = request.priorityAddress != null ? request.priorityAddress : guessInfo.priorityAddress;
            var guessUpdate = _unitOfWorks.GuessRepository.Update(guessInfo);
            if (guessUpdate == null)
            {
                throw new BadRequestException("Error in update info guess!");
            }
        }
        else
        {
            var guessModel = new GuessModel()
            {
                email = request.email.ToLower(),
                phone = request.phone != null ? request.phone : null,
                age = request.age != 0 ? request.age : 0,
                priorityAddress = request.priorityAddress != null ? request.priorityAddress : null,
                licenseType = request.licenseType != null ? request.licenseType : null,
                status = "Active",
                RoleID = 3,
                CreatedAt = DateTime.Now
            };
            var guessCreate = _mapper.Map<Guess>(guessModel);
            await _unitOfWorks.GuessRepository.AddAsync(guessCreate);
        }
        var result = await _unitOfWorks.GuessRepository.Commit();
        if (result > 0)
        {
            return true;
        }
        return false;
    }

}