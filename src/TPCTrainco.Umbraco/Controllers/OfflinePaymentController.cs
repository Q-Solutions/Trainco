using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using TPCTrainco.Umbraco.Extensions.Models;
using TPCTrainco.Umbraco.Extensions.Objects;
using Umbraco.Web.Mvc;

namespace TPCTrainco.Umbraco.Controllers
{
    public class OfflinePaymentController : SurfaceController
    {
        public ActionResult Index()
        {
            CheckoutCustomer checkoutCustomer = new CheckoutCustomer();
            checkoutCustomer.PaymentType = "credit";
            checkoutCustomer.OfflinePayment = true;
            return PartialView("OfflinePaymentForm", checkoutCustomer);
        }

        [NotChildAction]
        [HttpPost]
        public ActionResult HandleFormSubmit(CheckoutCustomer model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return CurrentUmbracoPage();
                Carts cartsObj = new Carts();
                CreditCardResult creditCardResult = cartsObj.ProcessCreditCard(model);
                if (creditCardResult.ErrorCode != 0)
                {
                    ViewBag.isError = true;
                    return CurrentUmbracoPage();
                }
                Registrations.EmailPaymentConfirmation(model); 
                return Redirect("/payment-success/");
            }
            catch (Exception ex)
            {
                ViewBag.isError = true;
                return CurrentUmbracoPage();
            }
        }
    }
}
