using System.Threading.Tasks;
using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Filters;
using OMS_5D_Tech.Services;
using OMS_5D_Tech.Models;
using System.Web.Http;

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

        [HttpPost]
        [Route("create")]
        [CustomAuthorize]
        public async Task<IHttpActionResult> CreateOrder([FromUri] int id, [FromBody] OrderDTO od)
        {
            var result = await _orderService.CreateOrderAsync(id , od);
            return Ok(result);
        }

        [HttpGet]
        [Route("detail")]
        [CustomAuthorize(Roles ="admin")]
        public async Task<IHttpActionResult> FindOrderById([FromUri] int id)
        {
            var result = await _orderService.FindOrderByIdAsync(id);
            return Ok(result);
        }
        
        [HttpPost]
        [Route("cancel")]
        public async Task<IHttpActionResult> CancelOrderById([FromUri] int id)
        {
            var result = await _orderService.CancelOrderAsync(id);
            return Ok(result);
        }
        [CustomAuthorize]

        [HttpGet]
        [Route("my-orders")]
        [CustomAuthorize]
        public async Task<IHttpActionResult> GetMyOrders([FromUri] int id, [FromUri] int? page = 1, [FromUri] int? pageSize = 10)
        {
            var result = await _orderService.GetMyOrders(id, page, pageSize);
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
        [CustomAuthorize]
        public async Task<IHttpActionResult> UpdateOrder([FromUri]int id, [FromBody]OrderDTO od)
        {
            var result = await _orderService.UpdateOrderAsync(id , od);
            return Ok(result);
        }
        
        [HttpGet]
        [Route("statistics")]
        [CustomAuthorize(Roles ="admin")]
        public async Task<IHttpActionResult> Statistics([FromUri] string status, [FromUri] string condition)
        {
            var result = await _orderService.Statistics(status , condition);
            return Ok(result);
        }
    }
}
