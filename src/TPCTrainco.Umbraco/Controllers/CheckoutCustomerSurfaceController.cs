﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;
using TPCTrainco.Umbraco.Extensions;
using TPCTrainco.Umbraco.Extensions.Models;
using TPCTrainco.Umbraco.Extensions.Objects;
using Umbraco.Web.Mvc;

namespace TPCTrainco.Umbraco.Controllers
{
    public class CheckoutCustomerSurfaceController : SurfaceController
    {
        public ActionResult Index()
        {
            CheckoutCustomer checkoutCustomer = new CheckoutCustomer();
            List<temp_Reg> cartList = null;
            string cartGuid = null;

            cartGuid = Carts.GetCartGuid(Session);

            if (false == string.IsNullOrWhiteSpace(cartGuid))
            {
                Carts cartsObj = new Carts();

                cartList = cartsObj.GetCart(cartGuid);

                if (cartList == null)
                {
                    return PartialView("CheckoutCustomer", checkoutCustomer);
                }
                else
                {
                    Session["CartId"] = cartGuid;

                    temp_Cust tempCust = null;

                    using (var db = new americantraincoEntities())
                    {
                        int regId = cartList[0].reg_ID;

                        tempCust = db.temp_Cust.Where(p => p.reg_ID == regId).FirstOrDefault();

                        if (tempCust != null)
                        {
                            checkoutCustomer = cartsObj.ConvertTempCustToCheckoutCustomer(tempCust);
                            checkoutCustomer.BillingDifferent = true;
                        }
                    }

                    return PartialView("CheckoutCustomer", checkoutCustomer);
                }
            }
            else
            {
                return PartialView("CheckoutCustomer", checkoutCustomer);
            }
        }


        [NotChildAction]
        [HttpPost]
        public ActionResult HandleFormSubmit(CheckoutCustomer model)
        {
            StringBuilder debug = new StringBuilder();

            debug.AppendLine("-=Customer Checkout=-\r\n");

            Carts cartsObj = new Carts();

            if (false == ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }
            else
            {
                debug.AppendLine("Loading cart...");

                CheckoutBilling checkoutBilling = new CheckoutBilling();
                List<temp_Reg> cartList = null;
                string cartGuid = null;

                cartGuid = Carts.GetCartGuid(Session);

                if (false == string.IsNullOrWhiteSpace(cartGuid))
                {
                    cartList = cartsObj.GetCart(cartGuid);

                    if (cartList == null)
                    {
                        debug.AppendLine("CART NOT FOUND!");

                        return Redirect("/search-seminars/");
                    }
                    else
                    {
                        Session["CartId"] = cartGuid;
                    }
                }

                if (cartList != null)
                {
                    // delete old temp_Cust
                    cartsObj.DeleteTempCust(cartList[0].reg_ID);

                    checkoutBilling.RegId = cartList[0].reg_ID;

                    if (model.PaymentType == "credit")
                    {
                        checkoutBilling.CCNumber = model.CCNumber.Replace("-", "");
                    }

                    temp_Cust tempCust = cartsObj.ConvertModelToTempCust(model, cartList);

                    if (tempCust != null)
                    {
                        tempCust.regTakenBy = "Customer Web";
                        tempCust.PastCustomer = "Not Sure";

                        if (model.PaymentType == "credit")
                        {
                            tempCust.ccNumber = checkoutBilling.CCNumber;
                        }

                        tempCust = cartsObj.SaveCheckoutDetails(tempCust);

                        if (tempCust != null)
                        {
                            return Redirect("/register/summary/");
                        }
                        else
                        {
                            cartsObj.SendCartErrorEmail("ERROR: 81\n\r tempCust == null");

                            return Redirect("/register/error/?error=81");
                        }
                    }
                    else
                    {
                        cartsObj.SendCartErrorEmail("ERROR: 82\n\r tempCust == null");
                        return Redirect("/register/error/?error=82");
                    }
                }
                else
                {
                    cartsObj.SendCartErrorEmail("ERROR: 83\n\r cartList == null");
                    return Redirect("/register/error/?error=83");
                }
            }
        }
    }
}