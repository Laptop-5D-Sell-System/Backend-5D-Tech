using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OMS_5D_Tech.Filters;
using OMS_5D_Tech.Interfaces;
using OMS_5D_Tech.Models;
using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Services;

namespace OMS_5D_Tech.Controllers
{
    [RoutePrefix("category")] 
    public class tbl_CategoriesController : ApiController
    {
        private readonly CategoryService _categoryService;
        private readonly DBContext _dbContext;
        public tbl_CategoriesController()
        {
            _dbContext = new DBContext();
            _categoryService = new CategoryService(_dbContext);
        }

        [HttpPost]
        [Route("create")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> CreateCategory([FromBody] CategoryDTO cat)
        {
            var result = await _categoryService.CreateCategoryAsync(cat);
            return Ok(result);
        }

        [HttpGet]
        [Route("detail")]
        public async Task<IHttpActionResult> FindCategoryById(int id)
        {
            var result = await _categoryService.FindCategoryByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("update")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> UpdateCategory([FromUri]int id , [FromBody] CategoryDTO cat)
        {
            var result = await _categoryService.UpdateCategoryAsync(id , cat);
            return Ok(result);
        }
        [HttpDelete]
        [Route("delete")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-all-categories")]
        public async Task<IHttpActionResult> getAllCategories()
        {
            var result = await _categoryService.getAllCategoriesAsync();
            return Ok(result);
        }
    }
}