﻿using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.ModelBinding;
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
        public async Task<IHttpActionResult> UpdateAccount()
        {
            var result = await _accountService.UpdateAccountAsync(HttpContext.Current.Request);
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
        public async Task<IHttpActionResult> ResetPassword([FromBody] AccountDTO acc)
        {
            var result = await _accountService.ResetPasswordAsync(acc);
            return Ok(result);
        }

        [HttpGet]
        [Route("verify-email")]
        public async Task<IHttpActionResult> VerifyEmail(string email)
        {
            var result = await _accountService.VerifyEmailAsync(email);

            var relativeUrl = result
                ? "/Notification/VerifySuccess"
                : "/Notification/VerifyFailed";

            var request = Request;
            var uriBuilder = new UriBuilder(request.RequestUri.Scheme, request.RequestUri.Host, request.RequestUri.Port, relativeUrl);

            return Redirect(uriBuilder.Uri.ToString());
        }

        [HttpPost]
        [Route("is-lock")]
        [CustomAuthorize(Roles ="admin")]
        public async Task<IHttpActionResult> IsLockAccount(int id)
        {
            var result = await _accountService.IsLock(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("logout")]
        [CustomAuthorize]
        public async Task<IHttpActionResult> LogOut([FromBody] LogoutDTO dto)
        {
            var result = await _accountService.LogoutAsync(dto);
            return Ok(result);
        }
    }
}
