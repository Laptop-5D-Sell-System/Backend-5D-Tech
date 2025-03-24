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
                    var isOrderExisting = await _dbContext.tbl_Orders.FirstOrDefaultAsync(o => o.id == od.id);
                    if (isOrderExisting != null)
                    {
                        return new { HttpStatus = HttpStatusCode.BadRequest, mess = "Đơn hàng đã tồn tại!" };
                    }

                    var productIds = od.OrderItems.Select(i => i.product_id).ToList();
                    var existingProducts = await _dbContext.tbl_Products
                        .Where(p => productIds.Contains(p.id))
                        .ToDictionaryAsync(p => p.id, p => p);

                    foreach (var item in od.OrderItems)
                    {
                        if (!existingProducts.ContainsKey(item.product_id ?? 0))
                        {
                            return new { HttpStatus = HttpStatusCode.BadRequest, mess = $"Sản phẩm ID {item.product_id} không tồn tại." };
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

                    _dbContext.tbl_Orders.Add(newOrder);
                    await _dbContext.SaveChangesAsync();

                    // Thêm sản phẩm vào đơn hàng
                    List<tbl_Order_Items> orderItems = new List<tbl_Order_Items>();
                    foreach (var item in od.OrderItems)
                    {
                        orderItems.Add(new tbl_Order_Items
                        {
                            order_id = newOrder.id,
                            product_id = item.product_id,
                            quantity = item.quantity,
                            price = existingProducts[item.product_id ?? 0].price // Lấy giá từ bảng Products
                        });
                    }

                    _dbContext.tbl_Order_Items.AddRange(orderItems);
                    await _dbContext.SaveChangesAsync();

                    newOrder.total = await _dbContext.tbl_Order_Items
                        .Where(oi => oi.order_id == newOrder.id)
                        .SumAsync(oi => oi.quantity * oi.price);

                    await _dbContext.SaveChangesAsync();

                    transaction.Commit();

                    return new { HttpStatus = HttpStatusCode.Created, mess = "Đặt hàng thành công!", OrderId = newOrder.id };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new { HttpStatus = HttpStatusCode.ServiceUnavailable, mess = "Lỗi xảy ra: " + ex.Message };
                }
            }
        }


        public Task<object> DeleteOrderAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<object> FindOrderByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<object> UpdateOrderAsync(OrderDTO od)
        {
            throw new NotImplementedException();
        }
    }
}