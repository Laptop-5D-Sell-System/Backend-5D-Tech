using OMS_5D_Tech.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace OMS_5D_Tech.Templates
{
    public class EmailTitle
    {
        public string SendNewPassword(string fullname , string newPassword)
        {
            string text = $@"
                            <h1>5D-Tech Laptop Shop System</h1>
                            <p>Xin chào, <strong>{fullname}</strong>,</p>
                            <p>Chúng tôi đã cập nhật lại mật khẩu của bạn.</p>
                            <p>Mật khẩu mới của bạn là <b>{newPassword}</b></p>
                            <p>Vui lòng trở về trang chủ để đăng nhập vào hệ thống.</p>
                            <br>
                            <p>Trân trọng,</p>
                            <p><strong>5D-Tech Laptop Shop System</strong></p>";
            return text;
        }

        public string SendVerifyEmail(string email)
        {
            string text = $@"
                            <h1>5D-Tech Laptop Shop System</h1>
                            <p>Xin chào, <strong>{email}</strong>,</p>
                            <p>Chúng tôi biết ơn bạn rất nhiều khi đã trở thành thành viên của hệ thống chúng tôi</p>
                            <p>Chúng tôi đã nhận thấy tài khoản của bạn đã đăng ký trong hệ thống của chúng tôi</p>
                            <p>Thời gian xác thực tài khoản chỉ có <b>3</b> phút</p>
                            <p>Để kích hoạt tài khoản của bạn , vui lòng nhấn vào nút để kích hoạt <a href='https://localhost:44303/auth/verify-email/?email={email}'>Kích hoạt tài khoản</a> để kích hoạt tài khoản</p>
                            <p>Nếu không phải bạn đăng sử dụng thì có thể bỏ qua tin nhắn này !</p>
                            <br>
                            <p>Trân trọng,</p>
                            <p><strong>5D-Tech Laptop Shop System</strong></p>";
            return text;
        }

        public string SendVerifyOrderEmail(string email, string orderId, string orderDate, string formattedItemsTable, decimal total)
        {
            return $@"
                <div style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #0073e6;'>5D-Tech Laptop Shop System</h2>
                    <p>Xin chào, {email},</p>
                    <p>Cảm ơn bạn đã đặt hàng tại 5D-Tech.</p>

                    <p><strong>Thông tin đơn hàng của bạn:</strong></p>
                    <ul>
                        <li><strong>Mã đơn hàng:</strong> {orderId}</li>
                        <li><strong>Ngày đặt hàng:</strong> {orderDate}</li>
                    </ul>

                    {formattedItemsTable}

                    <p><strong>Tổng tiền:</strong> {total:C}</p>

                    <p>Để kiểm tra trạng thái đơn hàng, vui lòng nhấn vào liên kết: <a href='#'>Xem đơn hàng</a></p>

                    <p>Nếu bạn không thực hiện giao dịch này, vui lòng liên hệ với bộ phận hỗ trợ của chúng tôi.</p>

                    <p style='margin-top:30px;'>Trân trọng,<br/>5D-Tech Laptop Shop System</p>
                </div>
            ";
        }


    }
}