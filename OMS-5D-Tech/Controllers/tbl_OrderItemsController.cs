using OMS_5D_Tech.Filters;
using OMS_5D_Tech.Models;
using OMS_5D_Tech.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace OMS_5D_Tech.Controllers
{
    [RoutePrefix("order-item")]
    public class tbl_OrderItemsController : ApiController
    {
        private readonly OrderItemService _orderItemService;

        private readonly DBContext _dbContext;

        public tbl_OrderItemsController()
        {
            _dbContext = new DBContext();
            _orderItemService = new OrderItemService(_dbContext);
        }

        [Route("detail")]
        [CustomAuthorize]
        public async Task<object> getOrderDetail(int id)
        {
            var result = await _orderItemService.GetOrderItemAsync(id);
            return Ok(result);
        }
    }
}