﻿using OMS_5D_Tech.Filters;
using OMS_5D_Tech.Interfaces;
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
    [RoutePrefix("payment")]
    public class tbl_PaymentController : ApiController
    {
        private readonly VnPayService _vpnPayService;
        private readonly DBContext _dbContext;
        public tbl_PaymentController()
        {
            _dbContext = new DBContext();
            _vpnPayService = new VnPayService(_dbContext);
        }

        [HttpPost]
        [Route("create")]
        [CustomAuthorize]
        public IHttpActionResult CreatePayment([FromBody] PaymentInformationModel model)
        {
            var url = _vpnPayService.CreatePaymentUrl(model, HttpContext.Current);

            return Ok(new
            {
                HttpStatus = 200,
                url = url
            });
        }

        [HttpGet]
        [Route("return")]
        public async Task<IHttpActionResult> PaymentReturn()
        {
            var result = await _vpnPayService.ProcessReturn(HttpContext.Current.Request.QueryString);

            var relativeUrl = result.IsSuccess
                ? "/Notification/PaymentSuccess"
                : "/Notification/PaymentFailed";

            var request = Request;
            var uriBuilder = new UriBuilder(request.RequestUri.Scheme, request.RequestUri.Host, request.RequestUri.Port, relativeUrl);

            return Redirect(uriBuilder.Uri.ToString());
        }

        [HttpGet]
        [Route("payment-by-COD")]
        public async Task<IHttpActionResult> PaymentCOD(int orderId)
        {
            var result = await _vpnPayService.paymentByCOD(orderId);          
            return Ok(result);
        }
    }
}