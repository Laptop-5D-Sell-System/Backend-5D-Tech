using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Interfaces;
using OMS_5D_Tech.Models;
using OMS_5D_Tech.Templates;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace OMS_5D_Tech.Services
{
    public class UserService : IUserService
    {
        private readonly DBContext _dbContext;

        public UserService(DBContext dbContext)
        {
            _dbContext = dbContext;
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

        public async Task<object> UpdateUserAsync(tbl_Users user)
        {
            try
            {
                var existingUser = await _dbContext.tbl_Users.FindAsync(user.id);
                if (existingUser == null)
                {
                    return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy người dùng!" };
                }

                
                if (user.last_name != null) existingUser.last_name = user.last_name;
                if (user.first_name != null) existingUser.first_name = user.first_name;
                if (user.dob != null) existingUser.dob = user.dob;
                if (user.phone_number != null) existingUser.phone_number = user.phone_number;
                if (user.address != null) existingUser.address = user.address;
                if (user.profile_picture != null) existingUser.profile_picture = user.profile_picture;
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