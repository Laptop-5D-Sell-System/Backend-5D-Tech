using MailKit;
using OMS_5D_Tech.DTOs;
using OMS_5D_Tech.Interfaces;
using OMS_5D_Tech.Libraries;
using OMS_5D_Tech.Models;
using OMS_5D_Tech.Templates;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;

namespace OMS_5D_Tech.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly DBContext _context;
        private readonly EmailTitle _emailTitle;

        public VnPayService(DBContext dBContext)
        {
            _emailTitle = new EmailTitle();
            _context = dBContext;
        }

        private async Task<int?> GetCurrentUserIdAsync()
        {
            var identity = (ClaimsIdentity)Thread.CurrentPrincipal?.Identity;
            if (identity == null) return null;

            if (!int.TryParse(identity.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int accountId))
                return null;

            var user = await _context.tbl_Users.FirstOrDefaultAsync(u => u.account_id == accountId);
            return user?.id;
        }

        public string CreatePaymentUrl(PaymentInformationModel order, HttpContext context)
        {
            var vnpUrl = ConfigurationManager.AppSettings["VNP_Url"];
            var returnUrl = ConfigurationManager.AppSettings["VNP_ReturnUrl"];
            var tmnCode = ConfigurationManager.AppSettings["VNP_TmnCode"];
            string hashSecret = ConfigurationManager.AppSettings["VNP_HashSecret"];

            VnPayLibrary vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", tmnCode);
            vnpay.AddRequestData("vnp_Amount", (order.Amount * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", context.Request.UserHostAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", order.OrderDescription);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", order.OrderId);

            return vnpay.CreateRequestUrl(vnpUrl, hashSecret);
        }

        public async Task<dynamic> ProcessReturn(NameValueCollection query)
        {
            var hashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
            var vnpay = new VnPayLibrary();

            foreach (string key in query)
            {
                if (key.StartsWith("vnp_"))
                    vnpay.AddResponseData(key, query[key]);
            }

            string orderIdStr = vnpay.GetResponseData("vnp_TxnRef");
            string responseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string amountStr = vnpay.GetResponseData("vnp_Amount");

            if (string.IsNullOrEmpty(orderIdStr) || string.IsNullOrEmpty(responseCode) || string.IsNullOrEmpty(amountStr))
            {
                return new { IsSuccess = false, mess = "Thiếu thông tin từ VNPAY" };
            }

            int orderId = int.Parse(orderIdStr);
            decimal amount = decimal.Parse(amountStr) / 100;

            var order = await _context.tbl_Orders.FindAsync(orderId);
            if (order == null)
            {
                return new { IsSuccess = false, mess = $"Đơn hàng #{orderId} không tồn tại." };
            }

            var existingPayment = await _context.tbl_Payments
                .FirstOrDefaultAsync(p => p.order_id == orderId && p.payment_method == "VNPAY");

            if (existingPayment != null)
            {
                return new { IsSuccess = true, mess = "Giao dịch đã tồn tại hoặc đã được xử lý" };
            }

            var payment = new tbl_Payments
            {
                order_id = orderId,
                payment_method = "VNPAY",
                payment_date = DateTime.Now,
                amount = (float)amount,
                status = responseCode == "00" ? "Success" : "Failed"
            };

            if (responseCode == "00")
            {
                order.status = "Done";

                var orderedProductIds = await _context.tbl_Order_Items
                    .Where(oi => oi.order_id == orderId)
                    .Select(oi => oi.product_id)
                    .ToListAsync();

                var userId = order.user_id;

                var cartItemsToRemove = await _context.tbl_Cart
                    .Where(c => c.user_id == userId && orderedProductIds.Contains(c.product_id))
                    .ToListAsync();

                if (cartItemsToRemove.Any())
                {
                    _context.tbl_Cart.RemoveRange(cartItemsToRemove);
                    await _context.SaveChangesAsync();
                }
            }

            _context.tbl_Payments.Add(payment);
            await _context.SaveChangesAsync();

            var user = await _context.tbl_Users.FirstOrDefaultAsync(_ => _.id == order.user_id);
            var account = await _context.tbl_Accounts.FirstOrDefaultAsync(_ => _.id == user.account_id);
            var email = account.email;

            var mail = new EmailService();
            mail.SendEmail(email, "Thanh toán thành công", _emailTitle.SendThankYouForPurchaseEmail(email, orderId.ToString(), amount));

            return new
            {
                IsSuccess = responseCode == "00",
                mess = responseCode == "00" ? "Thanh toán thành công" : "Thanh toán thất bại"
            };
        }
        public async Task<object> paymentByCOD(int orderId)
        {
            var order = await _context.tbl_Orders.FindAsync(orderId);
            if (order == null)
            {
                return new { HttpStatus = HttpStatusCode.NotFound, mess = "Không tìm thấy đơn hàng nào !" };
            }

            if (order.status == "Done")
            {
                return new { 
                    HttpStaus = HttpStatusCode.BadRequest, 
                    mess = "Đơn hàng này đã được thanh toán." ,
                    IsSuccess = true,
                };
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var payment = new tbl_Payments
                    {
                        order_id = orderId,
                        payment_method = "COD",
                        payment_date = DateTime.Now,
                        amount = (float)order.total,
                        status = "Success"
                    };

                    _context.tbl_Payments.Add(payment);
                    order.status = "Done";

                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    var userId = order.user_id;
                    var user = await _context.tbl_Users.FirstOrDefaultAsync(_ => _.id == userId);
                    var account = await _context.tbl_Accounts.FirstOrDefaultAsync(_ => _.id == user.account_id);
                    var email = account.email;

                    var mailService = new EmailService();
                    mailService.SendEmail(email, "Xác nhận thanh toán COD thành công", _emailTitle.SendThankYouForPurchaseEmail(email, orderId.ToString(), (decimal)order.total));

                    return new { HttpStatus = HttpStatusCode.OK , mess = "Thanh toán thành công" };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new { HttpStatus = HttpStatusCode.InternalServerError, mess = "Lỗi " + ex.Message };
                }
            }
        }
    }
}