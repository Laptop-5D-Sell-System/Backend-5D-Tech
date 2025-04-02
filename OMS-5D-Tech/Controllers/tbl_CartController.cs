using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Filters;
using OMS_5D_Tech.Models;
using OMS_5D_Tech.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace OMS_5D_Tech.Controllers
{
    [RoutePrefix("cart")]
    public class tbl_CartController : ApiController
    {
        private readonly CartService _cartService;

        private readonly DBContext _dbContext;
        public tbl_CartController()
        {
            _dbContext = new DBContext();
            _cartService = new CartService(_dbContext);
        }

        [HttpPost]
        [Route("create")]
        [CustomAuthorize]
        public async Task<IHttpActionResult> CreateCart(CartDTO cat)
        {
            var result = await _cartService.CreateCartAsync(cat);
            return Ok(result);
        }

        [HttpPost]
        [Route("update")]
        [CustomAuthorize]
        public async Task<IHttpActionResult> UpdateCart(int id , CartDTO cat)
        {
            var result = await _cartService.UpdateCartAsync(id , cat);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-my-cart")]
        [CustomAuthorize]
        public async Task<IHttpActionResult> MyCart()
        {
            var result = await _cartService.GetMyCart();
            return Ok(result);
        }

        [HttpGet]
        [Route("detail")]
        [CustomAuthorize]
        public async Task<IHttpActionResult> FindCartById(int id)
        {
            var result = await _cartService.FindCartByIdAsync(id);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [CustomAuthorize(Roles ="admin")]
        public async Task<IHttpActionResult> DeleteCart(int id)
        {
            var result = await _cartService.DeleteCartAsync(id);
            return Ok(result);
        }
    }
}
