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
        private readonly DBContext _dbContext;
        private readonly CloudianaryService _cloudinaryService;
        public ProductService(DBContext dbContext)
        {
            _dbContext = dbContext;
            _cloudinaryService = new CloudianaryService();
        }

        public async Task<object> CreateProductAsync(HttpRequest request)
        {
            try
            {
                var name = request.Form["name"];
                var description = request.Form["description"];
                var price = Convert.ToDecimal(request.Form["price"]);
                var stockQuantity = Convert.ToInt32(request.Form["stock_quantity"]);
                var categoryId = string.IsNullOrEmpty(request.Form["category_id"]) ? (int?)null : Convert.ToInt32(request.Form["category_id"]);

                var checkCat = await _dbContext.tbl_Categories.FindAsync(categoryId);
                if (checkCat == null)
                    categoryId = null;

                var check = await _dbContext.tbl_Products.FirstOrDefaultAsync(p => p.name == name);
                if (check != null)
                    return new { httpStatus = HttpStatusCode.BadRequest, mess = "Sản phẩm đã tồn tại!" };

                string imageUrl = null;
                if (request.Files.Count > 0)
                {
                    HttpPostedFile imageFile = request.Files["product_image"]; 
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        imageUrl = _cloudinaryService.UploadImage(imageFile);
                    }
                }

                var product = new tbl_Products
                {
                    name = name,
                    description = description,
                    price = price,
                    product_image = imageUrl,
                    stock_quantity = stockQuantity,
                    category_id = categoryId,
                    created_at = DateTime.Now
                };

                _dbContext.tbl_Products.Add(product);
                await _dbContext.SaveChangesAsync();

                return new { httpStatus = HttpStatusCode.Created, mess = "Tạo sản phẩm thành công!" };
            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Lỗi: " + ex.Message };
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
                    category_name = _dbContext.tbl_Categories
                        .Where(cat => cat.id == product.category_id)
                        .Select(cat => cat.name)
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
        public async Task<object> UpdateProductAsync(int id, HttpRequest request)
        {
            try
            {
                var check = await _dbContext.tbl_Products.FirstOrDefaultAsync(_ => _.id == id);
                if (check == null)
                    return new { httpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy sản phẩm !" };

                var name = request.Form["name"];
                var description = request.Form["description"];
                var priceStr = request.Form["price"];
                var stockQuantityStr = request.Form["stock_quantity"];
                var categoryIdStr = request.Form["category_id"];
                var imageFile = request.Files["product_image"];

                var checkName = await _dbContext.tbl_Products
                    .FirstOrDefaultAsync(_ => _.name == name && _.id != id);
                if (checkName != null)
                    return new { httpStatus = HttpStatusCode.BadRequest, mess = "Tên sản phẩm đã tồn tại !" };

                check.name = string.IsNullOrEmpty(name) ? check.name : name;
                check.description = string.IsNullOrEmpty(description) ? check.description : description;

                if (decimal.TryParse(priceStr, out var price))
                {
                    check.price = price;
                }

                if (int.TryParse(stockQuantityStr, out var stockQuantity))
                {
                    check.stock_quantity = stockQuantity;
                }

                if (int.TryParse(categoryIdStr, out var categoryId))
                {
                    var checkCat = await _dbContext.tbl_Categories.FirstOrDefaultAsync(_ => _.id == categoryId);
                    if (checkCat != null)
                        check.category_id = categoryId;
                }

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    check.product_image = _cloudinaryService.UploadImage(imageFile);
                }

                check.updated_at = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                return new { httpStatus = HttpStatusCode.OK, mess = "Sửa sản phẩm thành công !" };
            }
            catch (Exception ex)
            {
                return new { httpStatus = HttpStatusCode.InternalServerError, mess = "Có lỗi xảy ra: " + ex.Message };
            }
        }
    }
}