using OMS_5D_Tech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OMS_5D_Tech.Interfaces
{
    public interface IVnPayService
    {

        string CreatePaymentUrl(PaymentInformationModel order, HttpContext context);
    }
}
