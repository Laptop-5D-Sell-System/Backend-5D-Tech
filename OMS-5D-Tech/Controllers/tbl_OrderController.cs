using System.Threading.Tasks;
using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Filters;
using OMS_5D_Tech.Services;
using OMS_5D_Tech.Models;
using System.Web.Http;
using System.Web.ModelBinding;
using System.Collections.Generic;
using System;

namespace OMS_5D_Tech.Controllers
{
    [RoutePrefix("order")]
    public class tbl_OrderController : ApiController
    {
        private readonly OrderService _orderService;
        private readonly DBContext _dbContext;

        public tbl_OrderController()
        {
            _dbContext = new DBContext();
            _orderService = new OrderService(_dbContext);
        }

        [HttpGet]
        [Route("orders")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> getAllOrder()
        {
            var result = await _orderService.GetAllOrdersAsync();
            return Ok(result);
        }

        [HttpPost]
        [Route("create")]
        [CustomAuthorize]
        public async Task<IHttpActionResult> CreateOrder([FromBody] OrderDTO od)
        {
            var result = await _orderService.CreateOrderAsync(od);
            return Ok(result);
        }

        [HttpGet]
        [Route("detail")]
        public async Task<IHttpActionResult> FindOrderById([FromUri] int id)
        {
            var result = await _orderService.FindOrderByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("cancel")]
        [CustomAuthorize]
        public async Task<IHttpActionResult> CancelOrderById([FromUri] int id)
        {
            var result = await _orderService.CancelOrderAsync(id);
            return Ok(result);
        }
        [CustomAuthorize]

        [HttpGet]
        [Route("my-orders")]
        [CustomAuthorize]
        public async Task<IHttpActionResult> GetMyOrders([FromUri] string status)
        {
            var result = await _orderService.GetMyOrders(status);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> DeleteOrder([FromUri] int id)
        {
            var result = await _orderService.DeleteOrderAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("update")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> UpdateOrder([FromUri] int id, [FromBody] OrderDTO od)
        {
            var result = await _orderService.UpdateOrderAsync(id, od);
            return Ok(result);
        }

        [HttpGet]
        [Route("statistics")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> Statistics([FromUri] string status, [FromUri] string condition , [FromUri] DateTime? fromDate, [FromUri] DateTime? toDate)
        {
            var result = await _orderService.Statistics(status, condition , fromDate, toDate);
            return Ok(result);
        }

        [HttpPost]
        [Route("order-by-cart")]
        [CustomAuthorize]
        public async Task<IHttpActionResult> OrderByCart(List<CartDTO> cat)
        {
            var result = await _orderService.CreateOrderByCartAsync(cat);
            return Ok(result);
        }
    }
}
