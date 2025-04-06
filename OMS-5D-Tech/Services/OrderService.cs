using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Interfaces;
using OMS_5D_Tech.Models;
using OMS_5D_Tech.Templates;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Xml.Linq;

namespace OMS_5D_Tech.Services
{
    public class OrderService : IOrderService
    {
        private readonly DBContext _dbContext;
        private readonly EmailTitle _emailTitle;

        public OrderService(DBContext dbContext)
        {
            _emailTitle = new EmailTitle();
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


        public async Task<object> CreateOrderAsync(OrderDTO od)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    // Kiểm tra người dùng đã đăng nhập chưa
                    var userId = await GetCurrentUserIdAsync();
                    if (userId == null)
                        return new { HttpStatus = HttpStatusCode.Unauthorized, mess = "Vui lòng đăng nhập!" };

                    if (od.OrderItems == null || !od.OrderItems.Any())
                    {
                        return new { HttpStatus = HttpStatusCode.BadRequest, mess = "Không có sản phẩm nào trong đơn hàng." };
                    }

                    // Lấy các sản phẩm từ yêu cầu
                    var productIds = od.OrderItems.Select(i => i.product_id).ToList();

                    // Lấy các sản phẩm từ database dựa trên product_id
                    var existingProducts = await _dbContext.tbl_Products
                        .Where(p => productIds.Contains(p.id))
                        .ToDictionaryAsync(p => p.id, p => p);

                    // Kiểm tra các sản phẩm không tồn tại trong database
                    var missingProducts = productIds
                        .Where(p => p.HasValue)             
                        .Select(p => p.Value)                  
                        .Except(existingProducts.Keys)         
                        .ToList();
                    if (missingProducts.Any())
                    {
                        return new { HttpStatus = HttpStatusCode.BadRequest, mess = $"Sản phẩm ID {string.Join(", ", missingProducts)} không tồn tại." };
                    }

                    // Kiểm tra số lượng sản phẩm hợp lệ
                    foreach (var item in od.OrderItems)
                    {
                        if (item.quantity <= 0)
                        {
                            return new { HttpStatus = HttpStatusCode.BadRequest, mess = $"Số lượng sản phẩm ID {item.product_id} không hợp lệ." };
                        }

                        var product = existingProducts[item.product_id ?? 0];
                        if (product.stock_quantity < item.quantity)
                        {
                            return new { HttpStatus = HttpStatusCode.BadRequest, mess = $"Sản phẩm ID {item.product_id} không đủ hàng tồn kho." };
                        }
                    }

                    // Tạo đơn hàng mới
                    var newOrder = new tbl_Orders
                    {
                        user_id = userId,
                        order_date = DateTime.Now,
                        status = "Processing",
                        total = 0
                    };

                    // Thêm đơn hàng vào database
                    _dbContext.tbl_Orders.Add(newOrder);
                    await _dbContext.SaveChangesAsync();

                    // Danh sách các mục trong đơn hàng và tổng tiền
                    List<tbl_Order_Items> orderItems = new List<tbl_Order_Items>();
                    decimal totalAmount = 0;

                    foreach (var item in od.OrderItems)
                    {
                        var product = existingProducts[item.product_id ?? 0];
                        var price = product.price;
                        var subtotal = price * item.quantity;
                        totalAmount += subtotal;

                        // Thêm item vào danh sách orderItems
                        orderItems.Add(new tbl_Order_Items
                        {
                            order_id = newOrder.id,  // ID đơn hàng được tạo khi lưu
                            product_id = item.product_id ?? 0,
                            quantity = item.quantity,
                            price = price
                        });

                        // Cập nhật lại số lượng tồn kho sản phẩm
                        product.stock_quantity -= item.quantity;
                    }

                    // Cập nhật thông tin email với các sản phẩm trong đơn hàng
                    var culture = new CultureInfo("vi-VN");
                    var productInfoFormatted = @"
                        <table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%;'>
                            <thead style='background-color: #f2f2f2;'>
                                <tr>
                                    <th style='text-align:left;'>Tên sản phẩm</th>
                                    <th style='text-align:right;'>Số lượng</th>
                                    <th style='text-align:right;'>Đơn giá</th>
                                    <th style='text-align:right;'>Thành tiền</th>
                                </tr>
                            </thead>
                            <tbody>";

                            foreach (var item in orderItems)
                            {
                                var product = existingProducts[item.product_id ?? 0];
                                var subtotal = product.price * item.quantity;
                                productInfoFormatted += $@"
                                    <tr>
                                        <td>{product.name}</td>
                                        <td style='text-align:right;'>{item.quantity}</td>
                                        <td style='text-align:right;'>{product.price.ToString("N0", culture)} ₫</td>
                                        <td style='text-align:right;'>{subtotal.ToString("N0", culture)} ₫</td>
                                    </tr>";
                            }

                            productInfoFormatted += @"
                            </tbody>
                        </table>";

                    // Lưu các mục trong đơn hàng vào database
                    _dbContext.tbl_Order_Items.AddRange(orderItems);
                    await _dbContext.SaveChangesAsync();

                    // Cập nhật tổng tiền cho đơn hàng
                    newOrder.total = totalAmount;
                    await _dbContext.SaveChangesAsync();

                    // Lấy thông tin người dùng và tài khoản để gửi email xác nhận
                    var user = await _dbContext.tbl_Users.FirstOrDefaultAsync(_ => _.id == userId);
                    var account = await _dbContext.tbl_Accounts.FirstOrDefaultAsync(_ => _.id == user.account_id);

                    var emailService = new EmailService();

                    // Gửi email xác nhận đơn hàng
                    emailService.SendEmail(
                        account.email,
                        "Xác nhận đơn hàng",
                        _emailTitle.SendVerifyOrderEmail(
                            account.email,
                            newOrder.id.ToString(),
                            newOrder.order_date.ToString(),
                            productInfoFormatted,
                            newOrder.total
                        )
                    );

                    // Cam kết giao dịch
                    transaction.Commit();

                    // Trả về kết quả thành công
                    return new { HttpStatus = HttpStatusCode.Created, mess = "Đặt hàng thành công, vui lòng kiểm tra hòm thư của bạn", OrderId = newOrder.id };
                }
                catch (Exception ex)
                {
                    // Rollback nếu có lỗi xảy ra
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
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var order = await _dbContext.tbl_Orders
                        .Include(o => o.tbl_Order_Items)
                        .FirstOrDefaultAsync(o => o.id == id);

                    if (order == null)
                    {
                        return new { HttpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy đơn hàng!" };
                    }

                    if (order.status != "Pending")
                    {
                        return new { HttpStatus = HttpStatusCode.BadRequest, mess = "Không thể hủy đơn hàng đã được xử lý!" };
                    }

                    // Lấy danh sách sản phẩm từ order
                    var productIds = order.tbl_Order_Items.Select(oi => oi.product_id).ToList();

                    // Lấy danh sách sản phẩm từ DB
                    var products = await _dbContext.tbl_Products
                        .Where(p => productIds.Contains(p.id))
                        .ToDictionaryAsync(p => p.id); // Đưa về cặp key - value

                    // Hoàn lại số lượng tồn kho
                    foreach (var item in order.tbl_Order_Items)
                    {
                        if (products.ContainsKey(item.product_id ?? 0))
                        {
                            products[item.product_id ?? 0].stock_quantity += item.quantity;
                        }
                    }

                    // Đánh dấu đơn hàng là "Cancelled"
                    order.status = "Cancelled";
                    await _dbContext.SaveChangesAsync();

                    transaction.Commit();

                    return new { HttpStatus = HttpStatusCode.OK, mess = "Hủy đơn hàng thành công!" };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Lỗi xảy ra: " + ex.Message };
                }
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

                var orderItem = await _dbContext.tbl_Orders
                       .Include(o => o.tbl_Order_Items)
                       .FirstOrDefaultAsync(o => o.id == id);

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

                var productList = orderItem.tbl_Order_Items
                    .Select(oi => new
                    {
                        ProductName = oi.tbl_Products.name,
                        Quantity = oi.quantity
                    })
                    .ToList();

                return new
                {
                    HttpStatus = HttpStatusCode.OK,
                    mess = "Lấy đơn hàng thành công",
                    order = new
                    {
                        order.user_id,
                        order.order_date,
                        order.status,
                        products = productList,
                        total_quantity = totalQuantity,
                        order.total,
                    }
                };

            }
            catch (Exception ex)
            {
                return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Lỗi xảy ra: " + ex.Message };
            }
        }

        public async Task<object> UpdateOrderAsync(int id, OrderDTO od)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (id <= 0)
                    {
                        return new { HttpStatus = HttpStatusCode.BadRequest, mess = "ID đơn hàng không hợp lệ!" };
                    }

                    var existingOrder = await _dbContext.tbl_Orders
                        .Include(o => o.tbl_Order_Items)
                        .FirstOrDefaultAsync(o => o.id == id);

                    if (existingOrder == null)
                    {
                        return new { HttpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy đơn hàng!" };
                    }

                    if (!string.IsNullOrEmpty(od.status))
                    {
                        existingOrder.status = od.status;
                    }

                    if (od.OrderItems != null && od.OrderItems.Any())
                    {
                        var productIds = od.OrderItems.Select(i => i.product_id).ToList();
                        var existingProducts = await _dbContext.tbl_Products
                            .Where(p => productIds.Contains(p.id))
                            .ToDictionaryAsync(p => p.id, p => p);

                        var existingOrderItems = existingOrder.tbl_Order_Items.ToList();

                        foreach (var item in od.OrderItems)
                        {
                            if (!existingProducts.ContainsKey(item.product_id ?? 0))
                            {
                                return new { HttpStatus = HttpStatusCode.BadRequest, mess = $"Sản phẩm ID {item.product_id} không tồn tại." };
                            }

                            var product = existingProducts[item.product_id ?? 0];
                            var existingItem = existingOrderItems.FirstOrDefault(oi => oi.product_id == item.product_id);

                            if (existingItem != null)
                            {
                                if (item.quantity > 0)
                                {
                                    if (item.quantity > existingItem.quantity)
                                    {
                                        // Nếu số lượng tăng, trừ kho
                                        int diff = item.quantity - existingItem.quantity;
                                        if (product.stock_quantity < diff)
                                        {
                                            return new { HttpStatus = HttpStatusCode.BadRequest, mess = $"Sản phẩm ID {item.product_id} không đủ hàng trong kho!" };
                                        }
                                        product.stock_quantity -= diff;
                                    }
                                    else if (item.quantity < existingItem.quantity)
                                    {
                                        // Nếu số lượng giảm, hoàn kho
                                        int diff = existingItem.quantity - item.quantity;
                                        product.stock_quantity += diff;
                                    }

                                    existingItem.quantity = item.quantity;
                                }
                                else
                                {
                                    // Nếu số lượng <= 0, xóa khỏi đơn hàng và hoàn lại số lượng vào kho
                                    product.stock_quantity += existingItem.quantity;
                                    _dbContext.tbl_Order_Items.Remove(existingItem);
                                }
                            }
                            else
                            {
                                // Nếu sản phẩm chưa có trong đơn hàng, thêm mới và trừ kho
                                if (product.stock_quantity < item.quantity)
                                {
                                    return new { HttpStatus = HttpStatusCode.BadRequest, mess = $"Sản phẩm ID {item.product_id} không đủ hàng trong kho!" };
                                }
                                product.stock_quantity -= item.quantity;

                                _dbContext.tbl_Order_Items.Add(new tbl_Order_Items
                                {
                                    order_id = existingOrder.id,
                                    product_id = item.product_id,
                                    quantity = item.quantity,
                                    price = product.price
                                });
                            }
                        }

                        await _dbContext.SaveChangesAsync();

                        // Cập nhật tổng tiền đơn hàng
                        existingOrder.total = await _dbContext.tbl_Order_Items
                            .Where(oi => oi.order_id == existingOrder.id)
                            .SumAsync(oi => oi.quantity * oi.price);
                    }

                    await _dbContext.SaveChangesAsync();
                    transaction.Commit();

                    return new { HttpStatus = HttpStatusCode.OK, mess = "Cập nhật đơn hàng thành công!" };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Lỗi xảy ra: " + ex.Message };
                }
            }
        }


        public async Task<object> GetMyOrders(int? page, int? pageSize)
        {
            try
            {
                var id = await GetCurrentUserIdAsync();
                if(id == null)  return new {HttpStatus = HttpStatusCode.NotFound , mess = "Vui lòng đăng nhập !"};

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

        public async Task<object> Statistics(string status, string condition)
        {
            try
            {
                var query = _dbContext.tbl_Orders.Where(_ => _.status == status);

                condition = string.IsNullOrEmpty(condition) ? "day" : condition.ToLower();

                switch (condition.ToLower())
                {
                    case "day":
                        query = query.Where(_ => _.order_date.HasValue &&
                                                    _.order_date.Value.Year == DateTime.Now.Year &&
                                                    _.order_date.Value.Month == DateTime.Now.Month &&
                                                    _.order_date.Value.Day == DateTime.Now.Day);
                        break;

                    case "month":
                        query = query.Where(_ => _.order_date.HasValue &&
                                                    _.order_date.Value.Year == DateTime.Now.Year &&
                                                    _.order_date.Value.Month == DateTime.Now.Month);
                        break;

                    case "year":
                        query = query.Where(_ => _.order_date.HasValue &&
                                                    _.order_date.Value.Year == DateTime.Now.Year);
                        break;
                    default:
                        query = query.Where(_ => _.order_date.HasValue &&
                                                    _.order_date.Value.Year == DateTime.Now.Year &&
                                                    _.order_date.Value.Month == DateTime.Now.Month &&
                                                    _.order_date.Value.Day == DateTime.Now.Day);
                        break;
                }
                

                var result = await query.Select(_ => new { _.total }).ToListAsync();

                return new { HttpStatus = HttpStatusCode.OK, data = result };
            }
            catch (Exception ex)
            {
                return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Lỗi xảy ra: " + ex.Message };
            }
        }
    }
}