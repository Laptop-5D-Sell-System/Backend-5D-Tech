using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OMS_5D_Tech.Interfaces
{
    public interface IEmailService
    {
        bool SendEmail(string toEmail, string subject, string body);
    }
}