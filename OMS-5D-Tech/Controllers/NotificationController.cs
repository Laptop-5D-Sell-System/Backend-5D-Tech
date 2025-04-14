using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OMS_5D_Tech.Controllers
{
    public class NotificationController : Controller
    {
        public ActionResult VerifySuccess()
        {
            return View();
        }

        public ActionResult VerifyFailed()
        {
            return View();
        }

        public ActionResult PaymentSuccess()
        {
            return View();
        }

        public ActionResult PaymentFailed()
        {
            return View();
        }
    }
}