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
    public class LiveOnlineTrainingController : SurfaceController
    {
        public ActionResult Index(string trainingKey, int amount)
        {
            if (!Registrations.DoesOnlineTrainingExist(trainingKey))
                return null;
            CheckoutCustomer checkoutCustomer = new CheckoutCustomer();
            checkoutCustomer.OfflinePayment = false;
            checkoutCustomer.PaymentType = "credit";
            checkoutCustomer.InvoiceNumber = trainingKey;
            checkoutCustomer.Amount = amount;
            return PartialView("OnlineTrainingRegistration", checkoutCustomer);
        }

        [NotChildAction]
        [HttpPost]
        public ActionResult HandleFormSubmit(CheckoutCustomer model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.isCCError = true;
                    throw new Exception("");
                }
                if (Registrations.CheckRegistrantExists(model.InvoiceNumber, model.Email))
                {
                    ViewBag.isExistsError = true;
                    throw new Exception("");
                }
                Carts cartsObj = new Carts();
                CreditCardResult creditCardResult = cartsObj.ProcessCreditCard(model);
                if (creditCardResult.ErrorCode != 0)
                {
                    ViewBag.isCCError = true;
                    throw new Exception("");
                }
                string registrantKey = Registrations.RegisterTrainingAttendee(model.InvoiceNumber, new Dictionary<string, string>() { { "email", model.Email }, { "givenName", model.FirstName }, { "surname", model.LastName } });
                Registrations.AddGotoTrainingRegistrant(model, registrantKey);
                if (string.IsNullOrEmpty(registrantKey))
                    throw new Exception("");
                TempData["success"] = "You have registered for this training successfully";
                return RedirectToCurrentUmbracoPage();
            }
            catch (Exception ex)
            {
                ViewBag.isError = true;
                return CurrentUmbracoPage();
            }
            
        }
    }
}
