using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OMS_5D_Tech.Models
{
    public class PaymentInformationModel
    {
        public string OrderId { get; set; }
        public double Amount { get; set; }
        public string OrderDescription { get; set; }
        public string Name { get; set; }
    }
}