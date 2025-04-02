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
    [RoutePrefix("user")]
    public class tbl_UsersController : ApiController
    {
        private readonly UserService _userService;

        private readonly DBContext _dbContext;
        public tbl_UsersController()
        {
            _dbContext = new DBContext();
            _userService = new UserService(_dbContext);
        }
        [HttpGet]
        [Route("get-users")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> GetUser()
        {
            var result = await _userService.GetUser();
            return Ok(result);
        }

        [HttpGet]
        [Route("detail")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> FindUserByID(int id)
        {
            var result = await _userService.FindUserByIdAsync(id);
            return Ok(result);
        }


        [HttpPost]
        [Route("edit-user")]
        [CustomAuthorize]
        public async Task<IHttpActionResult> UpdateAccount(tbl_Users user)
        {
            var result = await _userService.UpdateUserAsync(user);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            return Ok(result);
        }
    }
}