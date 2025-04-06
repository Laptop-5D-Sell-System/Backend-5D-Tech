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

        public async Task<object> ProcessReturn(NameValueCollection query)
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
                return new { HttpStatus = HttpStatusCode.BadRequest, mess = "Thiếu thông tin từ VNPAY" };
            }

            int orderId = int.Parse(orderIdStr);
            decimal amount = decimal.Parse(amountStr) / 100;

            // Kiểm tra đơn hàng có tồn tại chưa
            var order = await _context.tbl_Orders.FindAsync(orderId);
            if (order == null)
            {
                return new { HttpStatus = HttpStatusCode.BadRequest, mess = $"Đơn hàng #{orderId} không tồn tại." };
            }

            // Kiểm tra xem thanh toán đã được ghi nhận chưa
            var existingPayment = await _context.tbl_Payments
                .FirstOrDefaultAsync(p => p.order_id == orderId && p.payment_method == "VNPAY");

            if (existingPayment != null)
            {
                return new { mess = "Giao dịch đã tồn tại hoặc đã được xử lý" };
            }


            // Thêm giao dịch mới
            var payment = new tbl_Payments
            {
                order_id = orderId,
                payment_method = "VNPAY",
                payment_date = DateTime.Now,
                amount = (float)amount,
                status = responseCode == "00" ? "Success" : "Failed"
            };

            // Update trạng thái cho order
            if(responseCode == "00")
            {
                order.status = "Done";
            }

            _context.tbl_Payments.Add(payment);
            await _context.SaveChangesAsync();

            var userId = _context.tbl_Orders.Where(_ => _.id == orderId).Select(_ => _.user_id).FirstOrDefault();
            var user = await _context.tbl_Users.FirstOrDefaultAsync(_ => _.id == userId);
            var account = await _context.tbl_Accounts.FirstOrDefaultAsync(_ => _.id == user.account_id);
            var email = account.email;

            // Gửi mail thanh toán thành công
            var mail = new EmailService();
            mail.SendEmail(email , "Thanh toán thành công" , _emailTitle.SendThankYouForPurchaseEmail(email, orderId.ToString(), amount));
            return new
            {
                HttpStatus = HttpStatusCode.OK,
                mess = (responseCode == "00") ? "Thanh toán thành công" : "Thanh toán thất bại"
            };
        }
    }
}