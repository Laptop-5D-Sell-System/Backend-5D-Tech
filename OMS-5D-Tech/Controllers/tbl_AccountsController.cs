using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BCrypt.Net;
using OMS_5D_Tech.Models;

namespace OMS_5D_Tech.Controllers
{
    public class tbl_AccountsController : Controller
    {
        private readonly IAccountService _accountService;

        public tbl_AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpGet]
        public ActionResult Index()
        {
            return Content("API 5D-Tech is running");
        }

        [HttpPost]
        [Route("auth/register")]
        public async Task<ActionResult> Register(tbl_Accounts acc)
        {
            var result = await _accountService.RegisterAsync(acc);
            return Json(result);
        }

        [HttpPost]
        [Route("auth/login")]
        public async Task<ActionResult> Login(string email, string password)
        {
            var result = await _accountService.LoginAsync(email, password);
            return Json(result);
        }

        [HttpGet]
        [Route("auth/detail")]
        public async Task<ActionResult> FindAccountByID(int id)
        {
            var result = await _accountService.FindAccountByIdAsync(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("auth/update")]
        public async Task<ActionResult> UpdateAccount(tbl_Accounts acc)
        {
            var result = await _accountService.UpdateAccountAsync(acc);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        [Route("auth/delete")]
        public async Task<ActionResult> DeleteAccount(int id)
        {
            var result = await _accountService.DeleteAccountAsync(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
