using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using OMS_5D_Tech.Interfaces;
using OMS_5D_Tech.Models;

namespace OMS_5D_Tech.Controllers
{
    [RoutePrefix("category")] 
    public class tbl_CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public tbl_CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        [Route("create")]
        public async Task<ActionResult> CreateCategory(tbl_Categories cat)
        {
            var result = await _categoryService.CreateCategoryAsync(cat);
            return Json(result);
        }

        [HttpGet]
        [Route("detail/{id:int}")]
        public async Task<ActionResult> FindCategoryById(int id)
        {
            var result = await _categoryService.FindCategoryByIdAsync(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("update")]
        public async Task<ActionResult> UpdateCategory(tbl_Categories cat)
        {
            var result = await _categoryService.UpdateCategoryAsync(cat);
            return Json(result);
        }
        [HttpDelete]
        [Route("delete/{id:int}")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            return Json(result);
        }
    }
}