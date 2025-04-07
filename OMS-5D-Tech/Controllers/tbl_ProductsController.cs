using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.SessionState;
using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Filters;
using OMS_5D_Tech.Models;
using OMS_5D_Tech.Services;

namespace OMS_5D_Tech.Controllers
{
    [RoutePrefix("product")]
    public class tbl_ProductsController : ApiController
    {
        public readonly ProductService _productService;
        public readonly DBContext _dbContext;

        public tbl_ProductsController()
        {
            _dbContext = new DBContext();
            _productService = new ProductService(_dbContext);
        }

        [HttpGet]
        [Route("all-products")]
        public async Task<IHttpActionResult> getAlLProducts(string sortOrder)
        {
            var result = await _productService.GetAllProducts(sortOrder);
            return Ok(result);
        }
        [HttpGet]
        [Route("detail")]
        public async Task<IHttpActionResult> DetailProduct(int id)
        {
            var result = await _productService.GetProductDetail(id);
            return Ok(result);
        }
        [HttpGet]
        [Route("products-by-catid")]
        public async Task<IHttpActionResult> getProductsByCatid(int id, string sortOrder)
        {
            var result = await _productService.GetProductsByCategoryID(id, sortOrder);
            return Ok(result);
        }
        
        [HttpGet]
        [Route("total-product-by-category")]
        public async Task<IHttpActionResult> getTotalProductByCateogory()
        {
            var result = await _productService.GetTotalProductByCateogory();
            return Ok(result);
        }
        [HttpPost]
        [Route("create")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> CreateProduct()
        {
            var result = await _productService.CreateProductAsync(HttpContext.Current.Request);
            return Ok(result);
        }
        [HttpDelete]
        [Route("delete")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            return Ok(result);
        }
        [HttpPost]
        [Route("update")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> UpdateProduct(int id)
        {
            var request = HttpContext.Current.Request;
            var result = await _productService.UpdateProductAsync(id, request);
            return Ok(result);
        }
    }
}