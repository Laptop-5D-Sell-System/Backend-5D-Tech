using System.Threading.Tasks;
using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Filters;
using OMS_5D_Tech.Services;
using OMS_5D_Tech.Models;
using System.Web.Http;
using System.Web.ModelBinding;

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
        [CustomAuthorize(Roles ="admin")]
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
