using AutoMapper;
using L_L.Business.Commons.Request;
using L_L.Business.Exceptions;
using L_L.Business.Models;
using L_L.Data.Entities;
using L_L.Data.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace L_L.Business.Services;

public class IdentityCardService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly CloudService _cloudService;
    private readonly IMapper _mapper;

    public IdentityCardService(UnitOfWorks unitOfWorks, CloudService cloudService, IMapper mapper)
    {
        _unitOfWorks = unitOfWorks;
        _cloudService = cloudService;
        _mapper = mapper;
    }

    public async Task<IdentityCardModel> UpdateIdentityCard(IdentityCardRequest request, int driverId)
    {
        // Lấy thông tin driver từ repository
        var driver = await _unitOfWorks.UserRepository.GetByIdAsync(driverId);

        // Kiểm tra xem thẻ căn cước đã tồn tại chưa
        var identityCardExist = _unitOfWorks.IdentityCardRepository
            .FindByCondition(x => x.UserId == driverId)
            .FirstOrDefault();

        if (identityCardExist != null)
        {
        if(request.id != null)    identityCardExist.id = request.id;
        if(request.name != null)     identityCardExist.name = request.name;
        if(request.dob != null)     identityCardExist.dob = request.dob;
        if(request.home != null)    identityCardExist.home = request.home;
        if(request.address != null)     identityCardExist.address = request.address;
        if(request.sex != null)     identityCardExist.sex = request.sex;
        if(request.nationality != null)     identityCardExist.nationality = request.nationality;
        if(request.doe != null)     identityCardExist.doe = request.doe;
        if(request.type != null)     identityCardExist.type = request.type;
        if(request.type_new != null)     identityCardExist.type_new = request.type_new;
        if(request.address_entities != null)     identityCardExist.address_entities = request.address_entities;
        if(request.features != null)     identityCardExist.features = request.features;
        if(request.issue_date != null)     identityCardExist.issue_date = request.issue_date;

            if (request.imageFront != null)
            {
                var updateImageFront = await _cloudService.UploadImageAsync(request.imageFront);
                identityCardExist.imageFront = updateImageFront.SecureUrl.ToString();
            }

            if (request.imageBack != null)
            {
                var updateImageBack = await _cloudService.UploadImageAsync(request.imageBack);
                identityCardExist.imageBack = updateImageBack.SecureUrl.ToString();
            }

            var identityCardUpdate = _unitOfWorks.IdentityCardRepository.Update(identityCardExist);
            var resultUpdate = await _unitOfWorks.IdentityCardRepository.Commit();
            if (resultUpdate > 0)
            {
                return _mapper.Map<IdentityCardModel>(identityCardUpdate);
            }

        }
        else
        {
            // Nếu thẻ căn cước không tồn tại, tạo mới
            var createIdentityModel = new IdentityCardModel()
            {
                id = request.id,
                name = request.name,
                dob = request.dob,
                home = request.home,
                address = request.address,
                sex = request.sex,
                nationality = request.nationality,
                doe = request.doe,
                type = request.type,
                type_new = request.type_new,
                address_entities = request.address_entities,
                features = request.features,
                issue_date = request.issue_date,
                UserId = driver.UserId
            };

            if (request.imageFront != null)
            {
                var newImageFront = await _cloudService.UploadImageAsync(request.imageFront);
                createIdentityModel.imageFront = newImageFront.SecureUrl.ToString();
            }

            if (request.imageBack != null)
            {
                var newImageBack = await _cloudService.UploadImageAsync(request.imageBack);
                createIdentityModel.imageBack = newImageBack.SecureUrl.ToString();
            }

            var createIdentityMapper = _mapper.Map<IdentityCard>(createIdentityModel);
            var createIdentityEntity = await _unitOfWorks.IdentityCardRepository.AddAsync(createIdentityMapper);
            var result = await _unitOfWorks.IdentityCardRepository.Commit();
            if (result > 0)
            {
                return _mapper.Map<IdentityCardModel>(createIdentityEntity);
            }
        }

        return null;
    }

    public async Task<IdentityCardModel> GetIdentityCard(int driverId)
    {
        var identityCard = await _unitOfWorks.IdentityCardRepository.FindByCondition(x => x.UserId == driverId)
            .FirstOrDefaultAsync();
        if (identityCard == null)
        {
            throw new BadRequestException("IdentityCard of user not found!");
        }
        return _mapper.Map<IdentityCardModel>(identityCard);
    }
}


