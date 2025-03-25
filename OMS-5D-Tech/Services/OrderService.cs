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
using System.Xml.Linq;

namespace OMS_5D_Tech.Services
{
    public class OrderService : IOrderService
    {
        private readonly DBContext _dbContext;

        public OrderService(DBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<object> CreateOrderAsync(int userId, OrderDTO od)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var productIds = od.OrderItems.Select(i => i.product_id).ToList();

                    var existingProducts = await _dbContext.tbl_Products
                        .Where(p => productIds.Contains(p.id))  
                        .ToDictionaryAsync(p => p.id, p => p); // Chuyển về cặp key - value 

                    var missingProducts = productIds.Where(p => p.HasValue).Select(p => p.Value).Except(existingProducts.Keys).ToList();
                    if (missingProducts.Any())
                    {
                        return new { HttpStatus = HttpStatusCode.BadRequest, mess = $"Sản phẩm ID {string.Join(", ", missingProducts)} không tồn tại." };
                    }

                    foreach (var item in od.OrderItems)
                    {
                        if (item.quantity <= 0)
                        {
                            return new { HttpStatus = HttpStatusCode.BadRequest, mess = $"Số lượng sản phẩm ID {item.product_id} không hợp lệ." };
                        }
                    }

                    // Tạo đơn hàng 
                    var newOrder = new tbl_Orders
                    {
                        user_id = userId,
                        order_date = DateTime.Now,
                        status = "Processing",
                        total = 0 
                    };

                    _dbContext.tbl_Orders.Add(newOrder);
                    await _dbContext.SaveChangesAsync(); 

                    // Thêm sản phẩm vào tbl_Order_Items
                    List<tbl_Order_Items> orderItems = new List<tbl_Order_Items>();
                    decimal totalAmount = 0;

                    foreach (var item in od.OrderItems)
                    {
                        var price = existingProducts[item.product_id ?? 0].price;
                        var subtotal = price * item.quantity;
                        totalAmount += subtotal;

                        orderItems.Add(new tbl_Order_Items
                        {
                            order_id = newOrder.id,
                            product_id = item.product_id,
                            quantity = item.quantity,
                            price = price
                        });
                    }

                    _dbContext.tbl_Order_Items.AddRange(orderItems);
                    await _dbContext.SaveChangesAsync();

                    // Cập nhật tổng tiền 
                    newOrder.total = totalAmount;
                    await _dbContext.SaveChangesAsync();

                    transaction.Commit();

                    return new { HttpStatus = HttpStatusCode.Created, mess = "Đặt hàng thành công!", OrderId = newOrder.id };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Lỗi xảy ra: " + ex.Message };
                }
            }
        }

        public async Task<object> DeleteOrderAsync(int id)
        {
            try
            {
                var isOrderExisting = await _dbContext.tbl_Orders.FirstOrDefaultAsync(_ => _.id == id);
                if (isOrderExisting == null)
                {
                    return new { HttpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy đơn hàng!" };
                }

                var orderItems = await _dbContext.tbl_Order_Items.Where(_ => _.order_id == id).ToListAsync();

                if (orderItems.Any())
                {
                    _dbContext.tbl_Order_Items.RemoveRange(orderItems);
                    await _dbContext.SaveChangesAsync();
                }

                _dbContext.tbl_Orders.Remove(isOrderExisting);
                await _dbContext.SaveChangesAsync();

                return new { HttpStatus = HttpStatusCode.OK, mess = "Xóa đơn hàng thành công!" };
            }
            catch (Exception ex)
            {
                return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Lỗi xảy ra: " + ex.Message };
            }
        }

        public async Task<object> CancelOrderAsync(int id)
        {
            try
            {
                var order = await _dbContext.tbl_Orders.FirstOrDefaultAsync(o => o.id == id);
                if (order == null)
                {
                    return new { HttpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy đơn hàng!" };
                }

                if (order.status != "Pending")
                {
                    return new { HttpStatus = HttpStatusCode.BadRequest, mess = "Không thể hủy đơn hàng đã được xử lý!" };
                }

                order.status = "Cancelled";
                await _dbContext.SaveChangesAsync();

                return new { HttpStatus = HttpStatusCode.OK, mess = "Hủy đơn hàng thành công!" };
            }
            catch (Exception ex)
            {
                return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Lỗi xảy ra: " + ex.Message };
            }
        }


        public async Task<object> FindOrderByIdAsync(int id)
        {
            try
            {
                var isOrderExisting = await _dbContext.tbl_Orders.FirstOrDefaultAsync(_ => _.id == id);
                if (isOrderExisting == null)
                {
                    return new { HttpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy đơn hàng!" };
                }

                var order = await _dbContext.tbl_Orders
                .Where(o => o.id == id)
                .Select(o => new
                {
                    o.user_id,
                    o.order_date,
                    o.status,
                    o.total
                }).FirstOrDefaultAsync();

                var totalQuantity = await _dbContext.tbl_Order_Items
                    .Where(oi => oi.order_id == id)
                    .SumAsync(oi => oi.quantity);

                if (order == null)
                {
                    return new { HttpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy đơn hàng!" };
                }

                return new
                {
                    HttpStatus = HttpStatusCode.OK,
                    mess = "Lấy đơn hàng thành công",
                    order = new
                    {
                        order.user_id,
                        order.order_date,
                        order.status,
                        order.total,
                        quantity = totalQuantity
                    }
                };

            }
            catch (Exception ex)
            {
                return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Lỗi xảy ra: " + ex.Message };
            }
        }

        //TODO
        public Task<object> UpdateOrderAsync(OrderDTO od)
        {
            throw new NotImplementedException();
        }
        //TODO
        public async Task<object> GetMyOrders(int id, int? page, int? pageSize)
        {
            try
            {
                if (id <= 0)
                {
                    return new { HttpStatus = HttpStatusCode.BadRequest, mess = "ID không hợp lệ!" };
                }

                int currentPage = page ?? 1; // Mặc định trang đầu tiên
                int currentPageSize = pageSize ?? 10; // Mặc định lấy 10 đơn hàng

                if (currentPage <= 0 || currentPageSize <= 0)
                {
                    return new { HttpStatus = HttpStatusCode.BadRequest, mess = "Page hoặc pageSize không hợp lệ!" };
                }

                var totalOrders = await _dbContext.tbl_Orders
                    .Where(o => o.user_id == id)
                    .CountAsync(); // Đếm tổng số đơn hàng

                var orders = await _dbContext.tbl_Orders
                    .Where(o => o.user_id == id)
                    .OrderByDescending(o => o.order_date) // Sắp xếp mới nhất lên đầu
                    .Skip((currentPage - 1) * currentPageSize) // Bỏ qua các đơn hàng trước đó
                    .Take(currentPageSize) // Lấy đúng số lượng cần
                    .Select(o => new
                    {
                        o.id,
                        o.order_date,
                        o.total,
                        o.status
                    }).ToListAsync();

                if (!orders.Any())
                {
                    return new { HttpStatus = HttpStatusCode.NoContent, mess = "Không có đơn hàng nào!" };
                }

                return new
                {
                    HttpStatus = HttpStatusCode.OK,
                    mess = "Lấy danh sách đơn hàng thành công!",
                    myOrders = orders,
                    totalOrders,
                    currentPage,
                    pageSize = currentPageSize,
                    totalPages = (int)Math.Ceiling((double)totalOrders / currentPageSize),
                };
            }
            catch (Exception ex)
            {
                return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Lỗi xảy ra: " + ex.Message };
            }
        }
    }
}