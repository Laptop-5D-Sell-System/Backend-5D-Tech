using System;
using System.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using OMS_5D_Tech.Interfaces;

namespace OMS_5D_Tech.Services
{
    public class EmailService: IEmailService
    {
        private readonly string smtpServer;
        private readonly int smtpPort;
        private readonly string senderEmail;
        private readonly string senderPassword;

        public EmailService()
        {
            smtpServer = ConfigurationManager.AppSettings["SmtpServer"];
            smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            senderEmail = ConfigurationManager.AppSettings["SenderEmail"];
            senderPassword = ConfigurationManager.AppSettings["SenderPassword"];
        }

        public bool SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("5D Tech Laptop Shop", senderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = body };
                message.Body = bodyBuilder.ToMessageBody();

                using (var smtp = new SmtpClient())
                {
                    smtp.Connect(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                    smtp.Authenticate(senderEmail, senderPassword);
                    smtp.Send(message);
                    smtp.Disconnect(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi gửi email: " + ex.ToString());
                throw new Exception("Lỗi khi gửi email: " + ex.Message);
            }
        }
    }
}