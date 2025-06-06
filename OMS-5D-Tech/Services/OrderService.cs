﻿using OMS_5D_Tech.DTOs;
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

                var userInfor = await _dbContext.tbl_Users.FirstOrDefaultAsync(_ => _.id == order.user_id);

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
                        Quantity = oi.quantity,
                        Image = oi.tbl_Products.product_image,
                        Price = oi.tbl_Products.price
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
                        address = userInfor.address,
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


        public async Task<object> GetMyOrders(string status)
        {
            try
            {
                var id = await GetCurrentUserIdAsync();
                if (id == null) return new { HttpStatus = HttpStatusCode.NotFound, mess = "Vui lòng đăng nhập !" };


                var totalOrders = await _dbContext.tbl_Orders
                    .Where(o => o.user_id == id && o.status == status)
                    .CountAsync(); // Đếm tổng số đơn hàng

                var orders = await _dbContext.tbl_Orders
                    .Where(o => o.user_id == id && o.status == status)
                    .OrderByDescending(o => o.order_date)
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
                };
            }
            catch (Exception ex)
            {
                return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Lỗi xảy ra: " + ex.Message };
            }
        }

        public async Task<object> Statistics(string status, string condition, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var now = DateTime.Now;
                condition = string.IsNullOrEmpty(condition) ? "day" : condition.ToLower();

                var query = _dbContext.tbl_Orders.AsQueryable();
                var totalOrders = await query.CountAsync();

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(_ => _.status == status);
                }

                // Áp dụng khoảng thời gian nếu có fromDate và toDate
                if (fromDate.HasValue && toDate.HasValue)
                {
                    var from = fromDate.Value.Date;
                    var to = toDate.Value.Date.AddDays(1).AddTicks(-1); // end of day
                    query = query.Where(_ => _.order_date.HasValue &&
                                             _.order_date.Value >= from &&
                                             _.order_date.Value <= to);
                }
                

                object result;

                switch (condition)
                {
                    case "day":
                        // Tổng doanh thu trong ngày hoặc trong khoảng
                        var totalDay = await query
                            .Where(_ => _.order_date.HasValue &&
                                (!fromDate.HasValue && !toDate.HasValue
                                    ? _.order_date.Value.Day == now.Day
                                    : true))
                            .SumAsync(_ => (double?)_.total) ?? 0.0;
                        var orderCountDay = await query.CountAsync();
                        var percentDay = totalOrders == 0 ? 0 : Math.Round(orderCountDay * 100.0 / totalOrders, 2);
                        result = new
                        {
                            HttpStatus = HttpStatusCode.OK,
                            mess = "Lấy thống kê theo " + condition + " thành công!",
                            total = totalDay,
                            totalOrders = orderCountDay,
                            percent = percentDay
                        };
                        break;

                    case "month":
                        // Doanh thu theo từng ngày trong tháng
                        var month = fromDate?.Month ?? now.Month;
                        var monthTo = toDate?.Month ?? now.Month;
                        var year = fromDate?.Year ?? now.Year;

                        var rawMonthData = await query
                            .Where(_ => _.order_date.HasValue &&
                                        _.order_date.Value.Month >= month &&
                                        _.order_date.Value.Month <= monthTo &&
                                        _.order_date.Value.Year == year)
                            .GroupBy(_ => new
                            {
                                y = _.order_date.Value.Year,
                                m = _.order_date.Value.Month,
                                d = _.order_date.Value.Day
                            })
                            .Select(g => new
                            {
                                Year = g.Key.y,
                                Month = g.Key.m,
                                Day = g.Key.d,
                                revenue = g.Sum(x => (double?)x.total) ?? 0.0
                            })
                            .ToListAsync();

                        var monthData = rawMonthData.Select(x => new
                        {
                            date = new DateTime(x.Year, x.Month, x.Day).ToString("yyyy-MM-dd"),
                            revenue = x.revenue
                        }).OrderBy(x => x.date)
                        .ToList();
                        var orderCountMonth = await query.CountAsync();
                        var percentMonth = totalOrders == 0 ? 0 : Math.Round(orderCountMonth * 100.0 / totalOrders, 2);
                        result = new
                        {
                            HttpStatus = HttpStatusCode.OK,
                            mess = "Lấy thống kê theo " + condition + " thành công!",
                            data = monthData,
                            totalOrders = orderCountMonth,
                            percent = percentMonth
                        };
                        break;

                    case "year":
                        // Doanh thu theo từng tháng trong năm
                        var selectedYear = fromDate?.Year ?? now.Year;

                        var rawYearData = await query
                            .Where(_ => _.order_date.HasValue &&
                                        _.order_date.Value.Year == selectedYear)
                            .GroupBy(_ => new { _.order_date.Value.Year, _.order_date.Value.Month })
                            .Select(g => new
                            {
                                year = g.Key.Year,
                                month = g.Key.Month,
                                revenue = g.Sum(x => (double?)x.total) ?? 0.0
                            })
                            .OrderBy(x => x.year).ThenBy(x => x.month)
                            .ToListAsync();

                        var yearData = rawYearData.Select(x => new
                        {
                            date = new DateTime(x.year, x.month, 1).ToString("yyyy-MM"),
                            revenue = x.revenue
                        }).ToList();
                        var orderCountYear = await query.CountAsync();
                        var percentYear = totalOrders == 0 ? 0 : Math.Round(orderCountYear * 100.0 / totalOrders, 2);
                        result = new
                        {
                            HttpStatus = HttpStatusCode.OK,
                            mess = "Lấy thống kê theo " + condition + " thành công!",
                            data = yearData,
                            totalOrders = orderCountYear,
                            percent = percentYear
                        };
                        break;

                    default:
                        result = new
                        {
                            HttpStatus = HttpStatusCode.BadRequest,
                            mess = "Giá trị condition không hợp lệ"
                        };
                        break;
                }

                return result;
            }
            catch (Exception ex)
            {
                return new
                {
                    HttpStatus = HttpStatusCode.InternalServerError,                  
                    mess = "Lỗi xảy ra: " + ex.Message
                };
            }
        }
        public async Task<object> CreateOrderByCartAsync(List<CartDTO> cartItems)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var userId = await GetCurrentUserIdAsync();
                    if (userId == null)
                        return new { HttpStatus = HttpStatusCode.Unauthorized, mess = "Vui lòng đăng nhập!" };

                    if (cartItems == null || !cartItems.Any())
                    {
                        return new { HttpStatus = HttpStatusCode.BadRequest, mess = "Giỏ hàng trống." };
                    }

                    var productIds = cartItems.Select(i => i.product_id).ToList();

                    var existingProducts = await _dbContext.tbl_Products
                        .Where(p => productIds.Contains(p.id))
                        .ToDictionaryAsync(p => p.id, p => p);

                    // Kiểm tra các sản phẩm không tồn tại trong database
                    var missingProducts = productIds
                        .Where(p => !existingProducts.ContainsKey((int)p))
                        .ToList();
                    if (missingProducts.Any())
                    {
                        return new { HttpStatus = HttpStatusCode.BadRequest, mess = $"Sản phẩm ID {string.Join(", ", missingProducts)} không tồn tại." };
                    }

                    // Kiểm tra số lượng sản phẩm hợp lệ và đủ hàng tồn kho
                    foreach (var cartItem in cartItems)
                    {
                        if (cartItem.quantity <= 0)
                        {
                            return new { HttpStatus = HttpStatusCode.BadRequest, mess = $"Số lượng sản phẩm ID {cartItem.product_id} không hợp lệ." };
                        }

                        if (!existingProducts.TryGetValue((int)cartItem.product_id, out var product))
                        {
                            return new { HttpStatus = HttpStatusCode.BadRequest, mess = $"Không tìm thấy sản phẩm ID {cartItem.product_id}." };
                        }

                        if (product.stock_quantity < cartItem.quantity)
                        {
                            return new { HttpStatus = HttpStatusCode.BadRequest, mess = $"Sản phẩm ID {cartItem.product_id} không đủ hàng tồn kho." };
                        }
                    }

                    var newOrder = new tbl_Orders
                    {
                        user_id = userId,
                        order_date = DateTime.Now,
                        status = "Processing",
                        total = 0
                    };

                    _dbContext.tbl_Orders.Add(newOrder);
                    await _dbContext.SaveChangesAsync();

                    // Danh sách các mục trong đơn hàng và tổng tiền
                    List<tbl_Order_Items> orderItems = new List<tbl_Order_Items>();
                    decimal totalAmount = 0;

                    foreach (var cartItem in cartItems)
                    {
                        var product = existingProducts[(int)cartItem.product_id];
                        var price = product.price;
                        var subtotal = price * cartItem.quantity;
                        totalAmount += subtotal;

                        // Thêm item vào danh sách orderItems
                        orderItems.Add(new tbl_Order_Items
                        {
                            order_id = newOrder.id,  // ID đơn hàng được tạo khi lưu
                            product_id = cartItem.product_id,
                            quantity = cartItem.quantity,
                            price = price
                        });

                        // Cập nhật lại số lượng tồn kho sản phẩm
                        product.stock_quantity -= cartItem.quantity;
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
                        var product = existingProducts[(int)item.product_id];
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

                    _dbContext.tbl_Order_Items.AddRange(orderItems);
                    await _dbContext.SaveChangesAsync();

                    newOrder.total = totalAmount;
                    await _dbContext.SaveChangesAsync();

                    var user = await _dbContext.tbl_Users.FirstOrDefaultAsync(_ => _.id == userId);
                    var account = await _dbContext.tbl_Accounts.FirstOrDefaultAsync(_ => _.id == user.account_id);

                    var emailService = new EmailService();

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

                    transaction.Commit();

                    return new { HttpStatus = HttpStatusCode.Created, mess = "Đơn hàng đã được tạo, vui lòng tiến hành thanh toán", OrderId = newOrder.id };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Lỗi xảy ra: " + ex.Message };
                }
            }
        }

        public async Task<object> GetAllOrdersAsync()
        {
            var orders = await _dbContext.tbl_Orders
                .Include(o => o.tbl_Order_Items.Select(oi => oi.tbl_Products))
                .Select(o => new OrderDTO
                {
                    id = o.id,
                    user_id = o.user_id,
                    order_date = o.order_date,
                    status = o.status,
                    total = o.total,
                    quantity = o.tbl_Order_Items.Sum(oi => oi.quantity),
                    OrderItems = o.tbl_Order_Items.Select(oi => new OrderItemDTO
                    {
                        id = oi.id,
                        order_id = oi.order_id,
                        product_id = oi.product_id,
                        quantity = oi.quantity,
                        price = oi.tbl_Products.price
                    }).ToList()
                })
                .ToListAsync();

            return new
            {
                HttpStatus = 200,
                mess = "Lấy ra toàn bộ đơn hàng thành công !",
                orders = orders
            };
        }

    }
}