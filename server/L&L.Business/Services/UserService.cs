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
using L_L.Business.Commons.Response;

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

        public async Task<List<UserModel>> GetAllUser()
        {
            var listUser = unitOfWorks.UserRepository.GetAll();
            return _mapper.Map<List<UserModel>>(listUser);
        }
        
        public async Task<GetAllUserPaginationResponse> GetAllUser(int page)
        {
            const int pageSize = 4; // Set the number of objects per page
            var users = await unitOfWorks.UserRepository.GetAll().OrderByDescending(x => x.UserId).ToListAsync();

            // Calculate total count of users
            var totalCount = users.Count();

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Get the users for the current page
            var pagedUsers = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Map to UserModel
            var userModels = _mapper.Map<List<UserModel>>(pagedUsers);

            return new GetAllUserPaginationResponse
            {
                data = userModels,
                pagination = new Pagination
                {
                    page = page,
                    totalPage = totalPages,
                    totalCount = totalCount
                }
            };
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
                    if (!string.IsNullOrEmpty(req.userName))
                    {
                        existedUser.UserName = req.userName;
                        existedUser.ModifyBy = req.userName;
                    }
                    if (!string.IsNullOrEmpty(req.passWord))
                    {
                        existedUser.Password = SecurityUtil.Hash(req.passWord);
                    }
                    if (!string.IsNullOrEmpty(req.fullName))
                    {
                        if (!IsValidFullName(req.fullName))
                        {
                            return null;
                        }
                        existedUser.FullName = req.fullName;
                    }
                    if (!string.IsNullOrEmpty(req.city))
                    {
                        existedUser.City = req.city;
                    }
                    if (!string.IsNullOrEmpty(req.gender))
                    {
                        existedUser.Gender = req.gender;
                    }
                    if (!string.IsNullOrEmpty(req.address))
                    {
                        existedUser.Address = req.address;
                    }
                    if (!string.IsNullOrEmpty(req.STK))
                    {
                        existedUser.STK = req.STK;
                    }
                    if (!string.IsNullOrEmpty(req.Bank))
                    {
                        existedUser.Bank = req.Bank;
                    }
                    if (req.birthDate.HasValue)
                    {
                        existedUser.BirthDate = req.birthDate;
                    }
                    if (!string.IsNullOrEmpty(req.phoneNumber))
                    {
                        if (!IsValidPhoneNumber(req.phoneNumber))
                        {
                            return null;
                        }
                        existedUser.PhoneNumber = req.phoneNumber;
                    }
                    if (req.avatar != null)
                    {
                        var uploadResult = await cloudService.UploadImageAsync(req.avatar);

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
                throw new UnAuthorizedException("Token has expired.");
            }

            string email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            var user = await unitOfWorks.UserRepository.FindByCondition(x => x.Email == email).FirstOrDefaultAsync();
            if (user is null)
            {
                throw new BadRequestException("Can not found User");
            }
            var userModel = _mapper.Map<UserModel>(user);
            /*if (userModel.AccountBalance != null)
            {
                var balance = SecurityUtil.Decrypt(userModel.AccountBalance);
                userModel.AccountBalance = balance;
            }*/

            return userModel;
        }

        public async Task<Dictionary<string, int>> GetAllRole()
        {
            var listUser = new Dictionary<string, int>();
            listUser.Add("Admin", 0);
            listUser.Add("Customer", 0);
            listUser.Add("Driver", 0);

            var listUserRole = unitOfWorks.UserRepository.GetAll();
            foreach (var user in listUserRole)
            {
                if (user.RoleID == 1)
                {
                    listUser["Admin"]++;
                }
                else if (user.RoleID == 2)
                {
                    listUser["Customer"]++;
                }
                else if (user.RoleID == 3)
                {
                    listUser["Driver"]++;
                }
            }

            return listUser;
        }

        public async Task<Dictionary<string, int>> GetUserAge()
        {
            // Initialize the dictionary for age groups
            var listUser = new Dictionary<string, int>
            {
                { "20<30", 0 },
                { "31<40", 0 },
                { "41+", 0 }
            };

            // Fetch all users from the repository
            var listUserRole = unitOfWorks.UserRepository.GetAll();

            // Iterate through the users and increment the count based on their age
            foreach (var user in listUserRole)
            {
                var birthDate = DateTime.Parse(user?.BirthDate.ToString());
                // Calculate the user's age
                var age = DateTime.Now.Year - birthDate.Year;

                // Adjust age if the birthday hasn't occurred yet this year
                if (user.BirthDate > DateTime.Now.AddYears(-age)) 
                {
                    age--;
                }

                // Increment the appropriate age group
                if (age >= 20 && age <= 30)
                {
                    listUser["20<30"]++;
                }
                else if (age >= 31 && age <= 40)
                {
                    listUser["31<40"]++;
                }
                else if (age >= 41)
                {
                    listUser["41+"]++;
                }
            }

            return listUser;
        }

        public async Task<List<DataMonth>> GetUserTypeLoginByMonth()
        {
            // Initialize a list for the monthly login data
            var DataMonth = new List<DataMonth>
            {
                new DataMonth { name = "Jan", googleAds = 0, normalAds = 0 },
                new DataMonth { name = "Feb", googleAds = 0, normalAds = 0 },
                new DataMonth { name = "Mar", googleAds = 0, normalAds = 0 },
                new DataMonth { name = "Apr", googleAds = 0, normalAds = 0 },
                new DataMonth { name = "May", googleAds = 0, normalAds = 0 },
                new DataMonth { name = "Jun", googleAds = 0, normalAds = 0 },
                new DataMonth { name = "Jul", googleAds = 0, normalAds = 0 },
                new DataMonth { name = "Aug", googleAds = 0, normalAds = 0 },
                new DataMonth { name = "Sep", googleAds = 0, normalAds = 0 },
                new DataMonth { name = "Oct", googleAds = 0, normalAds = 0 },
                new DataMonth { name = "Nov", googleAds = 0, normalAds = 0 },
                new DataMonth { name = "Dec", googleAds = 0, normalAds = 0 }
            };

            // Fetch all users from the repository
            var listUserTypeLogin =  unitOfWorks.UserRepository.GetAll();

            // Iterate through users and increment the count based on login type and month
            foreach (var user in listUserTypeLogin)
            {
                var createDate = DateTime.Parse(user?.CreateDate.ToString() ?? DateTime.Now.ToString());

                // Map month to corresponding index (1-based to 0-based)
                var monthIndex = createDate.Month - 1;

                // Increment the appropriate login type count
                if (user.TypeLogin == "Google") 
                {
                    DataMonth[monthIndex].googleAds++;
                }
                else if (user.TypeLogin == "Normal") 
                {
                    DataMonth[monthIndex].normalAds++;
                }                
                /*else if (user.TypeLogin == "Normal") 
                {
                    DataMonth[monthIndex].normalAds++;
                }*/
            }

            return DataMonth;
        }

        public async Task<bool> DeleteAccount(UserModel user)
        {
            var userDeleted = unitOfWorks.UserRepository.Remove(user.UserId);
            if (userDeleted != null)
            {
                var result = await unitOfWorks.UserRepository.Commit();
                if (result > 0)
                {
                    return true;
                }

                return false;
            }
            return false;
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
