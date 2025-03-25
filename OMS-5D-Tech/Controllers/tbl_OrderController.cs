using System.Threading.Tasks;
using System.Web.Mvc;
using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Filters;
using OMS_5D_Tech.Interfaces;
using OMS_5D_Tech.Models;

namespace OMS_5D_Tech.Controllers
{
    [RoutePrefix("order")]
    public class tbl_OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public tbl_OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [Route("create")]
        [CustomAuthorize]
        public async Task<ActionResult> CreateOrder(int id, OrderDTO od)
        {
            var result = await _orderService.CreateOrderAsync(id , od);
            return Json(result);
        }

        [HttpGet]
        [Route("detail/{id:int}")]
        [CustomAuthorize(Roles ="admin")]
        public async Task<ActionResult> FindOrderById(int id)
        {
            var result = await _orderService.FindOrderByIdAsync(id);
            return Json(result , JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        [Route("cancel/{id:int}")]
        public async Task<ActionResult> CancelOrderById(int id)
        {
            var result = await _orderService.CancelOrderAsync(id);
            return Json(result);
        }
        [CustomAuthorize]
        
        [HttpGet]
        [Route("my-orders/{id:int}")]
        public async Task<ActionResult> GetMyOrders(int id , int? page , int ?pageSize)
        {
            var result = await _orderService.GetMyOrders(id , page , pageSize);
            return Json(result , JsonRequestBehavior.AllowGet);
        }
        [CustomAuthorize]

        [HttpDelete]
        [Route("delete/{id:int}")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            var result = await _orderService.DeleteOrderAsync(id);
            return Json(result);
        }        
    }
}
