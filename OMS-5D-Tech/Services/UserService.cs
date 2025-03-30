using Microsoft.AspNet.Identity;
using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Interfaces;
using OMS_5D_Tech.Models;
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
                var user = await _dbContext.tbl_Users.FindAsync(id);
                if (user == null)
                {
                    return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy người dùng!" };
                }

                var account = await _dbContext.tbl_Accounts.FindAsync(user.account_id);
                if (account != null)
                {
                    _dbContext.tbl_Accounts.Remove(account);
                }

                _dbContext.tbl_Users.Remove(user);
                await _dbContext.SaveChangesAsync();

                return new { httpStatus = HttpStatusCode.OK, mess = "Đã xóa người dùng thành công!" };
            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }


        public async Task<object> FindUserByIdAsync(int id)
        {
            var user = await _dbContext.tbl_Users.Where(u => u.id == id).Select(u => new
            {
                u.id,
                u.first_name,
                u.last_name,
                u.address,
                u.created_at,
                u.updated_at,
                u.account_id,
                u.dob,
                infor_account = _dbContext.tbl_Accounts
               .Where(account => account.id == u.account_id)
               .Select(account => new { account.email, account.role, account.refresh_token_expiry })
               .FirstOrDefault()
            }).FirstOrDefaultAsync();

            if (user != null)
            {
                return new { httpStatus = HttpStatusCode.OK, mess = "Lấy thông tin người dùng thành công!", user };
            }
            return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy người dùng!" };
        }


        public async Task<object> GetUsers()
        {
            try
            {
                var users = await _dbContext.tbl_Users.Select(user => new
                {
                    user.id,
                    user.created_at,
                    user.updated_at,
                    user.first_name,
                    user.address,
                    user.last_name,
                    user.account_id,
                    user.dob,
                    infor_account = _dbContext.tbl_Accounts
               .Where(account => account.id == user.account_id)
               .Select(account => new {account.email, account.role, account.refresh_token_expiry})
               .FirstOrDefault()
                }).ToListAsync();
                return new { httpStatus = HttpStatusCode.OK, mess = "Lấy ra toàn bộ người dùng", users = users };
            }
            catch (Exception ex) {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra" + ex.Message };
            }
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

                var userDTO = new UserDTO
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

                return new { httpStatus = HttpStatusCode.OK, mess = "Sửa thông tin người dùng thành công!", user = userDTO };
            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }

    }
}