using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Interfaces;
using OMS_5D_Tech.Models;

namespace OMS_5D_Tech.Services
{
    public class ProductService : IProductService
    {
        public readonly DBContext _dbContext;
        public ProductService(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<object> CreateProductAsync(ProductDTO pro)
        {
            try
            {
                var check = await _dbContext.tbl_Products.FirstOrDefaultAsync(_ => _.name == pro.name);
                var checkCat = await _dbContext.tbl_Accounts.FirstOrDefaultAsync(_ => _.id == pro.category_id);
                if (checkCat == null)
                    pro.category_id = null;
                if(check != null) 
                    return new { httpStatus = HttpStatusCode.BadRequest, mess = "Đã tồn tại sản phẩm !" };

                var product = new tbl_Products
                {
                    name = pro.name,
                    description = pro.description,
                    price = pro.price,
                    product_image = pro.product_image,
                    stock_quantity = pro.stock_quantity,
                    category_id = pro.category_id,
                    created_at = DateTime.Now
                };
                _dbContext.tbl_Products.Add(product);
                await _dbContext.SaveChangesAsync();
                return new { httpStatus = HttpStatusCode.Created, mess = "Tạo sản phẩm thành công !" };

            }catch(Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }

        public async Task<object> DeleteProductAsync(int id)
        {
            try
            {
                var check = await _dbContext.tbl_Products.FirstOrDefaultAsync(_ => _.id == id);
                if (check == null)
                    return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy sản phẩm!" };

                _dbContext.tbl_Products.Remove(check);
                await _dbContext.SaveChangesAsync();
                return new { httpStatus = HttpStatusCode.OK, mess = "Đã xóa thành công sản phẩm !" };
            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }
        // Lấy hết sản phẩm có chọn lọc xếp theo giá hoặc không --> 3 endpoints
        public async Task<object> GetAllProducts(string sortOrder)
        {
            try
            {
                var query = _dbContext.tbl_Products.Select(product => new
                {
                    product.id,
                    product.name,
                    product.created_at,
                    product.updated_at,
                    product.description,
                    product.product_image,
                    product.price,
                    product.stock_quantity,
                    category_id = _dbContext.tbl_Categories
                        .Where(cat => cat.id == product.category_id)
                        .Select(cat => cat.id)
                        .FirstOrDefault()
                });
                // Sort by price 
                if (!String.IsNullOrEmpty(sortOrder))
                {
                    if (sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase))
                        query = query.OrderBy(p => p.price);

                    else if (sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase))
                        query = query.OrderByDescending(p => p.price);
                }
                var products = await query.ToListAsync();

                return new { httpStatus = HttpStatusCode.OK, mess = "Lấy thành công tất cả sản phẩm!", products = products };

            }catch(Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }

        public async Task<object> GetProductDetail(int id)
        {
            try
            {
                var check = await _dbContext.tbl_Products.FirstOrDefaultAsync(_ => _.id == id);
                if (check == null)
                    return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy sản phẩm !" };
                var product = new ProductDTO
                {
                    id = check.id,
                    name = check.name,
                    created_at = check.created_at,
                    updated_at = check.updated_at,
                    description = check.description,
                    product_image = check.product_image,
                    price = check.price,
                    category_id = check.category_id,
                    stock_quantity = check.stock_quantity,
                };

                return new { httpStatus = HttpStatusCode.OK, mess = "Tìm sản phẩm thành công !", product = product };

            }catch(Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }

        public async Task<object> GetProductsByCategoryID(int catid, string sortOrder)
        {
            try
            {
                var query = _dbContext.tbl_Products
                    .Where(p => p.category_id == catid)
                    .Select(product => new
                {
                    product.id,
                    product.name,
                    product.created_at,
                    product.updated_at,
                    product.description,
                    product.product_image,
                    product.price,
                    product.stock_quantity,
                    product.category_id
                } );
                if (!String.IsNullOrEmpty(sortOrder))
                {
                    if (sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase))
                        query = query.OrderBy(p => p.price);

                    else if (sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase))
                        query = query.OrderByDescending(p => p.price);
                }
                var products = await query.ToListAsync();

                return new { httpStatus = HttpStatusCode.OK, mess = "Lấy thành công tất cả sản phẩm theo danh mục!", products = products };

            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }

        public async Task<object> UpdateProductAsync(ProductDTO pro)
        {
            try
            {
                var check = await _dbContext.tbl_Products.FirstOrDefaultAsync(_ => _.id == pro.id);
                if (check == null)
                    return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy sản phẩm !" };
                // Check tên sản phẩm đã tồn tại
                var checkName = await _dbContext.tbl_Products.FirstOrDefaultAsync(_ => _.name == pro.name);
                if (checkName != null)
                    return new { httpStatus = HttpStatusCode.BadRequest, mess = "Tên sản phẩm đã tồn tại !" };
                // Check các fields k fill -> Lấy cũ
                check.name = pro.name ?? check.name;
                check.description = pro.description ?? check.description;
                check.product_image = pro.product_image ?? check.product_image;
                check.category_id = pro.category_id ?? check.category_id;
                if(pro.price != 0)
                    check.price = pro.price;
                if (pro.stock_quantity != 0)
                    check.stock_quantity = pro.stock_quantity;
                
                check.updated_at = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                var updatedProduct = new ProductDTO
                {
                    id = check.id,
                    name = check.name,
                    description = check.description,
                    product_image = check.product_image,
                    category_id = check.category_id,
                    price = check.price,
                    stock_quantity = check.stock_quantity,
                    updated_at = check.updated_at,
                    created_at = check.created_at
                };

                return new { httpStatus = HttpStatusCode.OK, mess = "Sửa sản phẩm thành công !", product = updatedProduct };

            }catch(Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }
    }
}