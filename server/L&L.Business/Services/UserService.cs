using AutoMapper;
using L_L.Business.Commons.Request;
using L_L.Business.Exceptions;
using L_L.Business.Models;
using L_L.Data.Entities;
using L_L.Data.Helpers;
using L_L.Data.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;

namespace L_L.Business.Services
{
    public class UserService
    {
        private readonly UnitOfWorks unitOfWorks;
        private readonly IMapper _mapper;
        private readonly CloudService cloudService;

        public UserService(UnitOfWorks unitOfWorks, IMapper mapper, CloudService cloudService)
        {
            this.unitOfWorks = unitOfWorks;
            _mapper = mapper;
            this.cloudService = cloudService;
        }

        public List<UserModel> GetAllUser()
        {
            var users = (unitOfWorks.UserRepository.GetAll()).ToList();
            return _mapper.Map<List<UserModel>>(users);
        }

        public async Task<UserModel> GetUserByEmail(string email)
        {
            return _mapper.Map<UserModel>(unitOfWorks.UserRepository.FindByCondition(x => x.Email == email).FirstOrDefault());
        }

        public async Task<UserModel> UpdateInfoUser(UserModel userUpdate, UpdateInfoRequest req)
        {
            try
            {
                var userEntity = _mapper.Map<User>(userUpdate);

                var existedUser = unitOfWorks.UserRepository.FindByCondition(x => x.Email == userEntity.Email).Include(x => x.UserRole).FirstOrDefault();

                if (existedUser != null)
                {
                    // Cập nhật thông tin người dùng từ req
                    if (!string.IsNullOrEmpty(req.UserName))
                    {
                        existedUser.UserName = req.UserName;
                        existedUser.ModifyBy = req.UserName;
                    }
                    if (!string.IsNullOrEmpty(req.Password))
                    {
                        existedUser.Password = SecurityUtil.Hash(req.Password);
                    }
                    if (!string.IsNullOrEmpty(req.FullName))
                    {
                        if (!IsValidFullName(req.FullName))
                        {
                            return null;
                        }
                        existedUser.FullName = req.FullName;
                    }
                    if (!string.IsNullOrEmpty(req.City))
                    {
                        existedUser.City = req.City;
                    }
                    if (!string.IsNullOrEmpty(req.Gender))
                    {
                        existedUser.Gender = req.Gender;
                    }
                    if (!string.IsNullOrEmpty(req.Address))
                    {
                        existedUser.Address = req.Address;
                    }
                    if (!string.IsNullOrEmpty(req.STK))
                    {
                        existedUser.STK = req.STK;
                    }
                    if (!string.IsNullOrEmpty(req.Bank))
                    {
                        existedUser.Bank = req.Bank;
                    }
                    if (req.BirthDate.HasValue)
                    {
                        existedUser.BirthDate = req.BirthDate;
                    }
                    if (!string.IsNullOrEmpty(req.PhoneNumber))
                    {
                        if (!IsValidPhoneNumber(req.PhoneNumber))
                        {
                            return null;
                        }
                        existedUser.PhoneNumber = req.PhoneNumber;
                    }
                    if (req.Avatar != null)
                    {
                        var uploadResult = await cloudService.UploadImageAsync(req.Avatar);

                        if (uploadResult.Error == null)
                        {
                            existedUser.Avatar = uploadResult.SecureUrl.ToString();
                        }
                        else
                        {
                            Console.WriteLine("Failed to upload avatar image");
                        }
                    }

                    //_mapper.Map(userEntity, existedUser);
                    existedUser.ModifyDate = DateTime.Now;

                    var user = unitOfWorks.UserRepository.Update(existedUser);

                    var result = await unitOfWorks.UserRepository.Commit();

                    var UserModel = new UserModel()
                    {
                        UserId = user.UserId,
                        UserName = user.UserName,
                        FullName = user.FullName,
                        Gender = user.Gender,
                        City = user.City,
                        Bank = user.Bank,
                        STK = user.STK,
                        Address = user.Address,
                        BirthDate = user.BirthDate,
                        ModifyBy = user.ModifyBy,
                        ModifyDate = user.ModifyDate,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Status = user.Status,
                        Avatar = user.Avatar,
                    };

                    if (result > 0)
                    {
                        return UserModel;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateCost(string cost, string email)
        {
            // Retrieve the user from the repository based on the email condition
            var user = unitOfWorks.UserRepository.FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            // Update only the AccountBalance
            /*            user.AccountBalance = SecurityUtil.Encrypt(cost);*/
            var text = SecurityUtil.Encrypt(cost);

            /*            var test = SecurityUtil.Decrypt(text);*/

            user.AccountBalance = text;

            // Update user in the repository
            unitOfWorks.UserRepository.Update(user);

            // Commit the changes
            var result = await unitOfWorks.UserRepository.Commit();

            return result > 0;
        }

        public async Task<UserModel> GetUserInToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new BadRequestException("Authorization header is missing or invalid.");
            }
            // Decode the JWT token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Check if the token is expired
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Token has expired.");
            }

            string email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            var user = await unitOfWorks.UserRepository.FindByCondition(x => x.Email == email).FirstOrDefaultAsync();
            if (user is null)
            {
                throw new BadRequestException("Can not found User");
            }
            var userModel = _mapper.Map<UserModel>(user);
            if (userModel.AccountBalance != null)
            {
                var balance = SecurityUtil.Decrypt(userModel.AccountBalance);
                userModel.AccountBalance = balance;
            }

            return userModel;
        }




        private bool IsValidFullName(string fullName)
        {
            return Regex.IsMatch(fullName, "^[a-zA-Z\\s]*$");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }


        private bool IsValidPhoneNumber(string phoneNumber)
        {
            return Regex.IsMatch(phoneNumber, "^0\\d{9,11}$");
        }
    }
}
