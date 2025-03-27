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
            return Content(HttpStatusCode.OK, new { HttpStatus = HttpStatusCode.OK, mess = "Lấy danh sách người dùng thành công !", data = result });
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Register(AccountDTO acc)
        {
            var result = await _accountService.RegisterAsync(acc);
            return Content(HttpStatusCode.OK, new { HttpStatus = HttpStatusCode.Created, mess = "Đăng ký thành công!"});
        }

        [HttpPost]
        [Route("signin-google")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GoogleLogin(string idToken)
        {
            var result = await _accountService.RegisterWithGoogleAsync(idToken);
            return Content(HttpStatusCode.OK, new { HttpStatus = HttpStatusCode.Created, mess = "Đăng ký thành công!" });
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Login([FromBody] AccountDTO login)
        {
            var result = await _accountService.LoginAsync(login.email, login.password_hash);
            return Content(HttpStatusCode.OK, new { HttpStatus = HttpStatusCode.OK, mess = "Đăng nhập thành công!", data = result });
        }


        [HttpGet]
        [Route("detail")]
        [CustomAuthorize(Roles = "admin")]
        public async Task<IHttpActionResult> FindAccountByID(int id)
        {
            var result = await _accountService.FindAccountByIdAsync(id);
            return Content(HttpStatusCode.OK, new { HttpStatus = HttpStatusCode.OK, mess = "Lấy thông tin người dùng thành công!" , data = result });
        }

        [HttpPost]
        [Route("change-password")]
        [CustomAuthorize]
        public async Task<IHttpActionResult> UpdateAccount(tbl_Accounts acc)
        {
            var result = await _accountService.UpdateAccountAsync(acc);
            return Content(HttpStatusCode.OK, new { HttpStatus = HttpStatusCode.OK, mess = "Cập nhật thông tin người dùng thành công!", data = result });
        }

        [HttpDelete]
        [Route("delete/{id:int}")]
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

        [HttpPost]
        [Route("verify-email")]
        public async Task<IHttpActionResult> VerifyEmail(string email)
        {
            var result = await _accountService.VerifyEmailAsync(email);
            return Ok(result);
        }
    }
}
