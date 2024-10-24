using AutoMapper;
using L_L.Business.Commons.Request;
using L_L.Business.Exceptions;
using L_L.Business.Models;
using L_L.Data.Entities;
using L_L.Data.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace L_L.Business.Services;

public class LicenseDriverService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;
    private readonly CloudService _cloudService;

    public LicenseDriverService(UnitOfWorks unitOfWorks, IMapper mapper, CloudService cloudService)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
        _cloudService = cloudService;
    }

    public async Task<LicenseDriverModel> UpdateLicenseDriver(LicenseDriverRequest request, int driverId)
    {
        // Lấy thông tin driver từ repository
        var driver = await _unitOfWorks.UserRepository.GetByIdAsync(driverId);

        // Kiểm tra xem giấy phép lái xe đã tồn tại chưa
        var licenseDriverExist = _unitOfWorks.LicenseDriverRepository
            .FindByCondition(x => x.UserId == driverId)
            .FirstOrDefault();

        if (licenseDriverExist != null)
        {
            // Nếu giấy phép lái xe đã tồn tại, cập nhật các thuộc tính
            if (request.id != null) licenseDriverExist.id = request.id;
            if (request.name != null) licenseDriverExist.name = request.name;
            if (request.dob != null) licenseDriverExist.dob = request.dob;
            if (request.nation != null) licenseDriverExist.nation = request.nation;
            if (request.address != null) licenseDriverExist.address = request.address;
            if (request.place_issue != null) licenseDriverExist.place_issue = request.place_issue;
            if (request.date != null) licenseDriverExist.date = request.date;
            if (request.doe != null) licenseDriverExist.doe = request.doe;
            if (request.classLicense != null) licenseDriverExist.classLicense = request.classLicense;
            if (request.type != null) licenseDriverExist.type = request.type;

            // Cập nhật ảnh nếu có
            if (request.imageFront != null)
            {
                var updateImageFront = await _cloudService.UploadImageAsync(request.imageFront);
                licenseDriverExist.imageFront = updateImageFront.SecureUrl.ToString();
            }

            if (request.imageBack != null)
            {
                var updateImageBack = await _cloudService.UploadImageAsync(request.imageBack);
                licenseDriverExist.imageBack = updateImageBack.SecureUrl.ToString();
            }

            // Cập nhật giấy phép lái xe vào repository
            var licenseDriverUpdate = _unitOfWorks.LicenseDriverRepository.Update(licenseDriverExist);
            var resultUpdate = await _unitOfWorks.LicenseDriverRepository.Commit();
            if (resultUpdate > 0)
            {
                return _mapper.Map<LicenseDriverModel>(licenseDriverUpdate);
            }
        }
        else
        {
            // Nếu giấy phép lái xe không tồn tại, tạo mới
            var createLicenseModel = new LicenseDriverModel()
            {
                id = request.id,
                name = request.name,
                dob = request.dob,
                nation = request.nation,
                address = request.address,
                place_issue = request.place_issue,
                date = request.date,
                doe = request.doe,
                classLicense = request.classLicense,
                type = request.type,
                UserId = driver.UserId
            };

            // Tải lên ảnh mặt trước nếu có
            if (request.imageFront != null)
            {
                var newImageFront = await _cloudService.UploadImageAsync(request.imageFront);
                createLicenseModel.imageFront = newImageFront.SecureUrl.ToString();
            }

            // Tải lên ảnh mặt sau nếu có
            if (request.imageBack != null)
            {
                var newImageBack = await _cloudService.UploadImageAsync(request.imageBack);
                createLicenseModel.imageBack = newImageBack.SecureUrl.ToString();
            }

            // Ánh xạ và lưu giấy phép lái xe mới vào repository
            var createLicenseMapper = _mapper.Map<LicenseDriver>(createLicenseModel);
            var createLicenseEntity = await _unitOfWorks.LicenseDriverRepository.AddAsync(createLicenseMapper);
            var result = await _unitOfWorks.LicenseDriverRepository.Commit();
            if (result > 0)
            {
                return _mapper.Map<LicenseDriverModel>(createLicenseEntity);
            }
        }

        return null;
    }
    
    public async Task<LicenseDriverModel> GetLicenseDriver(int driverId)
    {
        var licenseDriver = await _unitOfWorks.LicenseDriverRepository.FindByCondition(x => x.UserId == driverId)
            .FirstOrDefaultAsync();
        if (licenseDriver == null)
        {
            throw new BadRequestException("LicenseDriver of user not found!");
        }
        return _mapper.Map<LicenseDriverModel>(licenseDriver);
    }

}