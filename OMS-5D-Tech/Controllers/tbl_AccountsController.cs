using System.Threading.Tasks;
using System.Web.Mvc;
using OMS_5D_Tech.Models;

namespace OMS_5D_Tech.Controllers
{
    [RoutePrefix("auth")] // Đặt root cho api
    public class tbl_AccountsController : Controller
    {
        private readonly IAccountService _accountService;

        public tbl_AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register(tbl_Accounts acc)
        {
            var result = await _accountService.RegisterAsync(acc);
            return Json(result);
        }

        [HttpPost]
        [Route("signin-google")]
        public async Task<ActionResult> GoogleLogin(string idToken)
        {
            var result = await _accountService.RegisterWithGoogleAsync(idToken);
            return Json(result);
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login(string email, string password)
        {
            var result = await _accountService.LoginAsync(email, password);
            return Json(result);
        }

        [HttpGet]
        [Route("detail/{id:int}")]
        public async Task<ActionResult> FindAccountByID(int id)
        {
            var result = await _accountService.FindAccountByIdAsync(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("change-password")]
        public async Task<ActionResult> UpdateAccount(tbl_Accounts acc)
        {
            var result = await _accountService.UpdateAccountAsync(acc);
            return Json(result);
        }

        [HttpDelete]
        [Route("delete/{id:int}")]
        public async Task<ActionResult> DeleteAccount(int id)
        {
            var result = await _accountService.DeleteAccountAsync(id);
            return Json(result);
        }

        [HttpPost]
        [Route("reset-pasword")]
        public async Task<ActionResult> ResetPassword(string email)
        {
            var result = await _accountService.ResetPasswordAsync(email);
            return Json(result);
        }

        [HttpPost]
        [Route("verify-email")]
        public async Task<ActionResult> VerifyEmail(string email)
        {
            var result = await _accountService.VerifyEmailAsync(email);
            return Json(result);
        }

    }
}
