using AutoMapper;
using L_L.Business.Commons.Request;
using L_L.Business.Exceptions;
using L_L.Business.Models;
using L_L.Business.Ultils;
using L_L.Data.Entities;
using L_L.Data.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace L_L.Business.Services;

public class TransactionService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;
    private readonly CloudService _cloudService;

    public TransactionService(UnitOfWorks unitOfWorks, IMapper mapper, CloudService cloudService)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
        _cloudService = cloudService;
    }

    public async Task<bool> CreateTransactionRequest(CreateTransactionRequest request, string driverId)
    {
        var driver = await _unitOfWorks.UserRepository.GetByIdAsync(int.Parse(driverId));
        var admin = await _unitOfWorks.UserRepository.FindByCondition(x => x.RoleID == 1).FirstOrDefaultAsync();
        if (driver.Equals(null))
        {
            throw new BadRequestException("Driver not found");
        }

        if (decimal.Parse(request.amount) > decimal.Parse(driver.AccountBalance))
        {
            throw new BadRequestException("Request amount not larger than account balance!");
        }

        var createTransaction = new TransactionModel()
        {
            Amount = decimal.Parse(request.amount),
            Description = request.description,
            Note = request.note,
            Status = "Processing",
            DriverId = driver.UserId,
            AdminId = admin.UserId
        };
        var createTransactionEntity = _mapper.Map<Transaction>(createTransaction);
        await _unitOfWorks.TransactionRepository.AddAsync(createTransactionEntity);
        var result = await _unitOfWorks.TransactionRepository.Commit();
        if (result > 0)
        {
            return true;
        }
        return false;
    }

    public async Task<TransactionModel> UpdateStatusTransaction(UpdateStatusTransactionRequest request)
    {
        var transaction = await _unitOfWorks.TransactionRepository.GetByIdAsync(request.transactionId);
        if (transaction == null)
        {
            throw new BadRequestException("Transaction not found!");
        }

        transaction.Status = request.status.ToString();
        if (request.image != null)
        {
            var uploadResult = await _cloudService.UploadImageAsync(request.image);

            if (uploadResult.Error == null)
            {
                transaction.imagePay = uploadResult.SecureUrl.ToString();
            }
            else
            {
                Console.WriteLine("Failed to upload avatar image");
            }
        }
       var transactionUpdate = _unitOfWorks.TransactionRepository.Update(transaction);
       var result = await _unitOfWorks.TransactionRepository.Commit();
       if (result > 0)
       {
           return _mapper.Map<TransactionModel>(transactionUpdate);
       }
       return null;
    }
}