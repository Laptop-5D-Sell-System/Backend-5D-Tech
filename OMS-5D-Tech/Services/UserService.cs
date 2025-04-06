using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Interfaces;
using OMS_5D_Tech.Models;
using OMS_5D_Tech.Templates;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace OMS_5D_Tech.Services
{
    public class UserService : IUserService
    {
        private readonly DBContext _dbContext;
        private readonly CloudianaryService _cloudianaryService;

        public UserService(DBContext dbContext)
        {
            _dbContext = dbContext;
            _cloudianaryService = new CloudianaryService();
        }

        private async Task<int?> GetCurrentUserIdAsync()
        {
            var identity = (ClaimsIdentity)Thread.CurrentPrincipal?.Identity;
            if (identity == null) return null;

            if (!int.TryParse(identity.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int accountId))
                return null;

            var user = await _dbContext.tbl_Users.FirstOrDefaultAsync(u => u.account_id == accountId);
            return user?.id;
        }
        public async Task<object> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _dbContext.tbl_Users
                                           .Include(u => u.tbl_Accounts)  
                                           .Include(u => u.tbl_Feedbacks) 
                                           .FirstOrDefaultAsync(u => u.id == id);

                if (user == null)
                {
                    return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy người dùng!" };
                }
                var accounts = await _dbContext.tbl_Accounts.Where(acc => acc.id == user.account_id).ToListAsync();

                _dbContext.tbl_Feedbacks.RemoveRange(user.tbl_Feedbacks);

                _dbContext.tbl_Accounts.RemoveRange(accounts);

                _dbContext.tbl_Users.Remove(user);

                await _dbContext.SaveChangesAsync();

                return new { httpStatus = HttpStatusCode.OK, mess = "Xóa người dùng thành công" };
            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }




        public async Task<object> FindUserByIdAsync(int id)
        {
            var user = await _dbContext.tbl_Users
                .Where(_ => _.id == id)
                .Select(_ => new
                {
                    _.id,
                    _.account_id,
                    _.last_name,
                    _.first_name,
                    _.phone_number,
                    _.address,
                    _.dob,
                    _.profile_picture,
                    account = _dbContext.tbl_Accounts
                        .Where(acc => acc.id == _.account_id)
                        .Select(acc => new
                        {
                            acc.id,
                            acc.email,
                            acc.is_active,
                            acc.is_verified,
                            acc.role,
                            acc.created_at,
                            acc.updated_at
                        }).FirstOrDefault()
                }).FirstOrDefaultAsync();

            if (user != null)
            {
                return new { httpStatus = HttpStatusCode.OK, mess = "Lấy thông tin người dùng thành công!", user = user };
            }

            return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy người dùng!" };
        }


        public async Task<object> GetMyInfor()
        {
            var userId = await GetCurrentUserIdAsync();

            var user = await _dbContext.tbl_Users
                .Where(_ => _.id == userId)
                .Select(_ => new
                {
                    _.id,
                    _.account_id,
                    full_name = _.first_name + " " + _.last_name,
                    _.phone_number,
                    _.address,
                    _.dob,
                    _.profile_picture,
                    account = _dbContext.tbl_Accounts
                        .Where(acc => acc.id == _.account_id)
                        .Select(acc => new
                        {
                            acc.id,
                            acc.email,
                            acc.is_active,
                            acc.is_verified,
                            acc.role,
                            acc.created_at,
                            acc.updated_at
                        }).FirstOrDefault()
                }).FirstOrDefaultAsync();

            if (user != null)
            {
                return new { httpStatus = HttpStatusCode.OK, mess = "Lấy thông tin người dùng thành công!", user = user };
            }

            return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy người dùng!" };
        }

        public async Task<object> GetUser()
        {
            var users = await _dbContext.tbl_Users
                .Select(user => new
                {
                    user.id,
                    user.account_id,
                    user.last_name,
                    user.first_name,
                    user.dob,
                    user.phone_number,
                    user.address,
                    user.profile_picture,
                    account = _dbContext.tbl_Accounts
                        .Where(acc => acc.id == user.account_id)
                        .Select(acc => new
                        {
                            acc.id,
                            acc.email,
                            acc.is_active,
                            acc.is_verified,
                            acc.role,
                            acc.created_at,
                            acc.updated_at
                        }).FirstOrDefault()
                })
                .ToListAsync();

            return new { httpStatus = HttpStatusCode.OK, mess = "Danh sách toàn bộ người dùng", users = users };
        }

        public async Task<object> UpdateUserAsync(HttpRequest request)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                var existingUser = await _dbContext.tbl_Users.FindAsync(userId);
                var accountId = existingUser.account_id;
                var accountExisting = await _dbContext.tbl_Accounts.FindAsync(accountId);

                if (existingUser == null)
                {
                    return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy người dùng!" };
                }

                var first_name = request.Form["first_name"];
                var last_name = request.Form["last_name"]; 
                var dob = request.Form["dob"];
                var phone_number = request.Form["phone_number"];
                var address = request.Form["address"];
                var email = request.Form["email"];
                var password_hash = request.Form["password_hash"];

                if (!string.IsNullOrEmpty(first_name))
                    existingUser.first_name = first_name;

                if (!string.IsNullOrEmpty(last_name))
                    existingUser.last_name = last_name;

                if (!string.IsNullOrEmpty(dob) && DateTime.TryParse(dob, out DateTime parsedDob))
                    existingUser.dob = parsedDob;

                if (!string.IsNullOrEmpty(phone_number))
                    existingUser.phone_number = phone_number;

                if (!string.IsNullOrEmpty(address))
                    existingUser.address = address;
                
                if (!string.IsNullOrEmpty(email))
                {
                    var check_email = await _dbContext.tbl_Accounts.FirstOrDefaultAsync(_ => _.email == email);
                    if (check_email == null)
                    {
                        accountExisting.email = email;
                    }
                    else
                    {
                        return new { HttpStatus = HttpStatusCode.BadRequest, mess = "Email đã được sử dụng !" };
                    }
                }

                if (!string.IsNullOrEmpty(password_hash))
                    accountExisting.password_hash = BCrypt.Net.BCrypt.HashPassword(password_hash);

                if (request.Files.Count > 0)
                {
                    var imageFile = request.Files["profile_picture"];
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        var imageUrl = _cloudianaryService.UploadImage(imageFile);
                        existingUser.profile_picture = imageUrl;
                    }
                }

                existingUser.updated_at = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                var userDto = new UserDTO
                {
                    id = existingUser.id,
                    account_id = existingUser.account_id,
                    first_name = existingUser.first_name,
                    last_name = existingUser.last_name,
                    dob = existingUser.dob,
                    phone_number = existingUser.phone_number,
                    address = existingUser.address,
                    profile_picture = existingUser.profile_picture,
                    created_at = existingUser.created_at,
                    updated_at = existingUser.updated_at
                };

                return new { httpStatus = HttpStatusCode.OK, mess = "Sửa thông tin người dùng thành công!", user = userDto };
            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }

    }
}