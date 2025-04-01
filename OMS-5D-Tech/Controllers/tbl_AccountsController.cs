using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Filters;
using OMS_5D_Tech.Models;

namespace OMS_5D_Tech.Controllers
{
    [RoutePrefix("auth")]
    public class tbl_AccountsController : ApiController
    {
        private readonly AccountService _accountService;

        private readonly DBContext _dbContext;
        public tbl_AccountsController()
        {
            _dbContext = new DBContext();
            _accountService = new AccountService(_dbContext);
        }

        [HttpGet]
        [Route("get-accounts")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> GetAccounts()
        {
            var result = await _accountService.GetAccount();
            return Ok(result);
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Register(AccountDTO acc)
        {
            var result = await _accountService.RegisterAsync(acc);
            return Ok(result);
        }

        [HttpPost]
        [Route("signin-google")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GoogleLogin(string idToken)
        {
            var result = await _accountService.RegisterWithGoogleAsync(idToken);
            return Ok(result);
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Login([FromBody] AccountDTO login)
        {
            var result = await _accountService.LoginAsync(login.email, login.password_hash);
            return Ok(result);
        }


        [HttpGet]
        [Route("detail")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> FindAccountByID(int id)
        {
            var result = await _accountService.FindAccountByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("change-password")]
        [CustomAuthorize]
        public async Task<IHttpActionResult> UpdateAccount(tbl_Accounts acc)
        {
            var result = await _accountService.UpdateAccountAsync(acc);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> DeleteAccount(int id)
        {
            var result = await _accountService.DeleteAccountAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("reset-password")]
        public async Task<IHttpActionResult> ResetPassword(string email)
        {
            var result = await _accountService.ResetPasswordAsync(email);
            return Ok(result);
        }

        [HttpGet]
        [Route("verify-email")]
        public async Task<IHttpActionResult> VerifyEmail(string email)
        {
            var result = await _accountService.VerifyEmailAsync(email);
            if (result)
            {
                return Ok(new { message = "Kích hoạt tài khoản thành công !" });
            }
            return Ok(new { message = "Kích hoạt tài khoản thất bại !" });
        }

        [HttpPost]
        [Route("is-lock")]
        [CustomAuthorize(Roles ="admin")]
        public async Task<IHttpActionResult> IsLockAccount(int id)
        {
            var result = await _accountService.IsLock(id);
            return Ok(result);
        }
    }
}
