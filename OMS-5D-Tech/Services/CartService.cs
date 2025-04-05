using Google.Apis.Drive.v3.Data;
using Microsoft.IdentityModel.Tokens;
using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Interfaces;
using OMS_5D_Tech.Models;
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
    public class CartService : ICartService
    {
        private readonly DBContext _dbContext;

        public CartService(DBContext dbContext)
        {
            _dbContext = dbContext;
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

        public async Task<object> CreateCartAsync(CartDTO cat)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                if (userId == null)
                    return new { HttpStatus = HttpStatusCode.Unauthorized, mess = "Người dùng không hợp lệ!" };

                var product = await _dbContext.tbl_Products.FindAsync(cat.product_id);
                if (product == null)
                    return new { HttpStatus = HttpStatusCode.NotFound, mess = "Sản phẩm không tồn tại!" };

                var existingCart = await _dbContext.tbl_Cart.FirstOrDefaultAsync(c => c.user_id == userId && c.product_id == cat.product_id);

                if(cat.quantity <= 0)
                {
                    return new { HttpStatus = HttpStatusCode.BadRequest, mess = "Số lượng sản phẩm phải lớn hơn 0" };
                }
                else if(cat.quantity > product.stock_quantity)
                {
                    return new { HttpStatus = HttpStatusCode.BadRequest, mess = "Số lượng sản phẩm lớn hơn số lượng tồn kho !" };
                }

                if (existingCart != null)
                {
                    existingCart.quantity += cat.quantity;
                }
                else
                {
                    var newCart = new tbl_Cart { user_id = (int)userId, quantity = cat.quantity, product_id = cat.product_id };
                    _dbContext.tbl_Cart.Add(newCart);
                }
                await _dbContext.SaveChangesAsync();
                return new { HttpStatus = HttpStatusCode.Created, mess = "Thêm sản phẩm vào giỏ hàng thành công!" };
            }
            catch (Exception ex)
            {
                return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Xảy ra lỗi: " + ex.Message };
            }
        }

        public async Task<object> DeleteCartAsync(int id)
        {
            try
            {
                var cartItem = await _dbContext.tbl_Cart.FindAsync(id);
                if (cartItem == null)
                    return new { HttpStatus = HttpStatusCode.NotFound, mess = "Sản phẩm trong giỏ hàng không tồn tại!" };

                _dbContext.tbl_Cart.Remove(cartItem);
                await _dbContext.SaveChangesAsync();

                return new { HttpStatus = HttpStatusCode.OK, mess = "Xóa sản phẩm khỏi giỏ hàng thành công!" };
            }
            catch (Exception ex)
            {
                return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Xảy ra lỗi: " + ex.Message };
            }
        }

        public async Task<object> FindCartByIdAsync(int id)
        {
            try
            {
                var cartItem = await _dbContext.tbl_Cart
                    .Include(c => c.tbl_Products)
                    .FirstOrDefaultAsync(c => c.id == id);

                if (cartItem == null)
                    return new { HttpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy giỏ hàng!" };

                return new
                {
                    HttpStatus = HttpStatusCode.OK,
                    mess = "Lấy giỏ hàng thành công!",
                    data = new
                    {
                        cartItem.id,
                        cartItem.user_id,
                        cartItem.product_id,
                        cartItem.quantity,
                        product_name = cartItem.tbl_Products?.name
                    }
                };
            }
            catch (Exception ex)
            {
                return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Xảy ra lỗi: " + ex.Message };
            }
        }

        public async Task<object> UpdateCartAsync(int id, CartDTO cat)
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                if (userId == null)
                    return new { HttpStatus = HttpStatusCode.Unauthorized, mess = "Vui lòng đăng nhập !" };

                var cartItem = await _dbContext.tbl_Cart
                    .FirstOrDefaultAsync(c => c.user_id == userId && c.product_id == cat.product_id && c.id == id);

                if (cartItem == null)
                    return new { HttpStatus = HttpStatusCode.NotFound, mess = "Sản phẩm không có trong giỏ hàng!" };

                if (cat.quantity <= 0)
                {
                    _dbContext.tbl_Cart.Remove(cartItem);
                    await _dbContext.SaveChangesAsync();
                    return new { HttpStatus = HttpStatusCode.OK, mess = "Đã xóa sản phẩm khỏi giỏ hàng vì số lượng <= 0!" };
                }

                var product = await _dbContext.tbl_Products.FindAsync(cat.product_id);
                if (product == null)
                    return new { HttpStatus = HttpStatusCode.NotFound, mess = "Sản phẩm không tồn tại!" };
                    
                if(product.stock_quantity < 0)
                {
                    return new { HttpStatus = HttpStatusCode.BadRequest, mess = "Số lượng sản phẩm lớn hơn số lượng tồn kho !" };
                }

                cartItem.product_id = product.id;
                cartItem.quantity = cat.quantity;
                await _dbContext.SaveChangesAsync();

                return new { HttpStatus = HttpStatusCode.OK, mess = "Cập nhật giỏ hàng thành công!" };
            }
            catch (Exception ex)
            {
                return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Xảy ra lỗi: " + ex.Message };
            }
        }

        public async Task<object> GetMyCart()
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                if (userId == null)
                {
                    return new { HttpStatus = HttpStatusCode.Unauthorized, mess = "Người dùng chưa đăng nhập!" };
                }

                var cartItems = await _dbContext.tbl_Cart
                    .Where(c => c.user_id == userId)
                    .Include(c => c.tbl_Products) 
                    .Select(c => new
                    {
                        c.id,
                        c.product_id,
                        product_name = c.tbl_Products.name,
                        product_price = c.tbl_Products.price,
                        product_image = c.tbl_Products.product_image,
                        c.quantity
                    })
                    .ToListAsync();

                if (cartItems == null || cartItems.Count == 0)
                {
                    return new { HttpStatus = HttpStatusCode.NotFound, mess = "Giỏ hàng trống!" };
                }

                return new { HttpStatus = HttpStatusCode.OK, mess = "Lấy giỏ hàng thành công!", data = cartItems };
            }
            catch (Exception ex)
            {
                return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Xảy ra lỗi: " + ex.Message };
            }
        }
    }
}