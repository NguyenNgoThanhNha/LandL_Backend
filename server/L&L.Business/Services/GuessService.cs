using AutoMapper;
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

}