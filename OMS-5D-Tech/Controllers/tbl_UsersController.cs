using OMS_5D_Tech.Filters;
using OMS_5D_Tech.Interfaces;
using OMS_5D_Tech.Models;
using OMS_5D_Tech.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace OMS_5D_Tech.Controllers
{
    [RoutePrefix("user")] // Đặt root cho api
    public class tbl_UsersController : Controller
    {
        private readonly IUserService _userService;

        public tbl_UsersController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        [Route("get-users")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<ActionResult> GetUsers()
        {
            var result = await _userService.GetUsers();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("detail/{id:int}")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<ActionResult> FindUserByID(int id)
        {
            var result = await _userService.FindUserByIdAsync(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        [Route("delete/{id:int}")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            return Json(result);
        }

        [HttpPost]
        [Route("edit-user")]
        [CustomAuthorize]
        public async Task<ActionResult> UpdateUser(tbl_Users user)
        {
            var result = await _userService.UpdateUserAsync(user);
            return Json(result);
        }
    }
}