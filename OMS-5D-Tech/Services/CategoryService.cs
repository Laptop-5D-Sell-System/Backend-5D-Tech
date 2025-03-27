using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using MimeKit.Tnef;
using OMS_5D_Tech.Interfaces;
using OMS_5D_Tech.Models;

namespace OMS_5D_Tech.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly DBContext _dbContext;

        public CategoryService(DBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<object> CreateCategoryAsync(tbl_Categories cat)
        {
            try
            {
                var check = await _dbContext.tbl_Categories.AnyAsync(_ => _.name == cat.name);
                if (check)
                {
                    return new { httpStatus = HttpStatusCode.BadRequest, mess = "Đã tồn tại thể loại !" };
                }
                _dbContext.tbl_Categories.Add(cat);
                _dbContext.SaveChanges();
                return new { httpStatus = HttpStatusCode.Created, mess = "Tạo thể loại thành công !" };

            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }

        public async Task<object> FindCategoryByIdAsync(int id)
        {
            try
            {
                var check = await _dbContext.tbl_Categories.FindAsync(id);
                if (check==null)
                {
                    return new { httpStatus = HttpStatusCode.BadRequest, mess = "Không tồn tại thể loại!" };
                }
                return new { HttpStatusCode = HttpStatusCode.OK, mess = "Tìm thể loại thành công !", category = check };
            }catch(Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }

        public async Task<object> UpdateCategoryAsync(tbl_Categories cat)
        {
            try
            {
                var check = await _dbContext.tbl_Categories.FindAsync(cat.id);
                if(check == null)
                {
                    return new { httpStatus = HttpStatusCode.BadRequest, mess = "Không tìm thấy thể loại !" };
                }
                var checkName = await _dbContext.tbl_Categories.AnyAsync(_ => _.name == cat.name);
                if (checkName)
                {
                    return new { httpStatus = HttpStatusCode.BadRequest, mess = "Thể loại đã tồn tại, không thể sửa!" };
                }
                if(cat.name != null)
                    check.name = cat.name;
                if(cat.description != null)
                    check.description = cat.description;
                await _dbContext.SaveChangesAsync();
                return new { htppStatus = HttpStatusCode.OK, mess = "Sửa thể loại thành công !", category = check };
            }catch(Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message};
            }
        }

        public async Task<object> DeleteCategoryAsync(int id)
        {
            try
            {
                var check = await _dbContext.tbl_Categories.FindAsync(id);
                if(check == null)
                {
                    return new { httpStatus = HttpStatusCode.BadRequest, mess = "Không tìm thấy thể loại !" };
                }
                _dbContext.tbl_Categories.Remove(check);
                await _dbContext.SaveChangesAsync();
                return new { htppStatus = HttpStatusCode.OK, mess = "Xóa thành công thể loại !" };
            }catch(Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }
    }

}