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
    public class OrderItemService : IOrderItemService
    {
        private readonly DBContext _dbContext;
        public OrderItemService(DBContext dBContext)
        {
            _dbContext = dBContext;            
        }
        public async Task<object> GetOrderItemAsync(int id)
        {
            try
            {
                var isOrderExisting = await _dbContext.tbl_Order_Items.FirstOrDefaultAsync(_ => _.order_id == id);
                if (isOrderExisting == null)
                {
                    return new { HttpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy đơn hàng phù hợp !" };
                }
                var orderDetails = _dbContext.tbl_Order_Items
                .Where(item => item.order_id == id)
                .Join(_dbContext.tbl_Products,
                      item => item.product_id,
                      product => product.id,
                      (item, product) => new { item, product })
                .Join(_dbContext.tbl_Categories,
                      combined => combined.product.category_id,
                      category => category.id,
                      (combined, category) => new
                      {
                          OrderId = combined.item.order_id,
                          ProductId = combined.item.product_id,
                          ProductName = combined.product.name,
                          CategoryName = category.name ,
                          ProductDescription = combined.product.description,
                          ProductImage = combined.product.product_image,
                          Quantity = combined.item.quantity,
                          Price = combined.item.price,
                          Total = combined.item.quantity * combined.item.price
                      })
                .ToList();


                return new {HttpStatus = HttpStatusCode.OK, mess = "Lấy chi tiết đơn hàng thành công !" , orderDetails };
            }
            catch (Exception ex)
            {
                return new {HttpStatus = HttpStatusCode.InternalServerError , mess = "Xảy ra lỗi " + ex.Message};
            }
        }
    }
}