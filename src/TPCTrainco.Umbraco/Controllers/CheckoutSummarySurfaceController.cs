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
    public class CheckoutSummarySurfaceController : SurfaceController
    {
        public ActionResult Index()
        {
            CheckoutDetails checkoutDetails = new CheckoutDetails();
            List<temp_Reg> tempRegList = null;
            List<temp_Att> tempAttList = null;
            string cartGuid = null;

            cartGuid = Carts.GetCartGuid(Session);

            if (false == string.IsNullOrWhiteSpace(cartGuid))
            {
                Carts cartsObj = new Carts();

                tempRegList = cartsObj.GetCart(cartGuid);

                if (tempRegList == null)
                {
                    return PartialView("CheckoutSummary", checkoutDetails);
                }
                else
                {
                    Session["CartId"] = cartGuid;

                    temp_Cust tempCust = null;

                    using (var db = new americantraincoEntities())
                    {
                        int regId = tempRegList[0].reg_ID;

                        tempCust = db.temp_Cust.Where(p => p.reg_ID == regId).FirstOrDefault();
                        tempAttList = db.temp_Att.Where(p => p.reg_ID == regId).ToList();

                        if (tempCust != null && tempAttList != null && tempAttList.Count > 0)
                        {
                            checkoutDetails.tempRegList = tempRegList;
                            checkoutDetails.tempAttList = tempAttList;
                            checkoutDetails.tempCust = tempCust;
                        }
                    }

                    return PartialView("CheckoutSummary", checkoutDetails);
                }
            }
            else
            {
                return PartialView("CheckoutSummary", checkoutDetails);
            }


        }


        [HttpPost]
        public ActionResult HandleFormSubmit(CheckoutDetails model)
        {
            Carts cartsObj = new Carts();
            StringBuilder debug = new StringBuilder();

            debug.AppendLine("-=Cart Checkout=-\r\n");

            if (false == ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }
            else
            {
                CheckoutDetails checkoutDetails = new CheckoutDetails();
                List<temp_Reg> tempRegList = null;
                List<temp_Att> tempAttList = null;
                string cartGuid = null;

                debug.AppendLine("Loading cart...");

                cartGuid = Carts.GetCartGuid(Session);

                if (false == string.IsNullOrWhiteSpace(cartGuid))
                {
                    tempRegList = cartsObj.GetCart(cartGuid);

                    if (tempRegList == null)
                    {
                        debug.AppendLine("CART NOT FOUND!");

                        cartsObj.SendCartErrorEmail("ERROR: 94\n\rtempRegList == null\r\n\r\nDebug:\r\n" + debug.ToString());

                        return Redirect("/search-seminars/?error=94");
                    }
                    else
                    {
                        Session["CartId"] = cartGuid;
                    }
                }

                if (tempRegList != null)
                {
                    debug.AppendLine("Begin processing cart.");

                    try
                    {
                        debug.AppendLine("cartGuid: " + cartGuid);

                        Session["CartId"] = cartGuid;

                        temp_Cust tempCust = null;

                        using (var db = new americantraincoEntities())
                        {
                            debug.AppendLine("tempRegList: " + (tempRegList == null ? "NULL!" : "Count: " + tempRegList.Count));

                            int regId = tempRegList[0].reg_ID;

                            debug.AppendLine("regId: " + regId);

                            CheckoutBilling checkoutBilling = null;
                            CreditCardResult creditCardResult = null;
                            REGISTRATION reg = null;

                            tempCust = db.temp_Cust.Where(p => p.reg_ID == regId).FirstOrDefault();
                            tempAttList = db.temp_Att.Where(p => p.reg_ID == regId).ToList();

                            debug.AppendLine("tempCust: " + (tempCust == null ? "NULL!" : "ID: " + tempCust.ID));
                            debug.AppendLine("tempAttList: " + (tempAttList == null ? "NULL!" : "Count: " + tempAttList.Count));

                            checkoutBilling = new CheckoutBilling();

                            checkoutBilling.RegId = regId;
                            checkoutBilling.CCNumber = tempCust.ccNumber;

                            debug.AppendLine("checkoutBilling: " + (checkoutBilling == null ? "NULL!" : "RegId: " + checkoutBilling.RegId));

                            if (tempCust != null && tempAttList != null && tempAttList.Count > 0)
                            {
                                checkoutDetails.tempRegList = tempRegList;
                                checkoutDetails.tempAttList = tempAttList;
                                checkoutDetails.tempCust = tempCust;

                                bool isError = false;
                                bool isAlreadyProcessed = false;

                                debug.AppendLine("tempCust.payMethod: " + tempCust.payMethod);

                                // check for CC already processed: [isCardProcessed()] true -> Success page
                                if (tempCust.payMethod == "Credit Card" && checkoutBilling != null)
                                {
                                    debug.AppendLine("Check if credit card is already processed...");

                                    // card processed already?
                                    isAlreadyProcessed = cartsObj.CreditCardAlreadyProcessed(tempCust);

                                    debug.AppendLine("isAlreadyProcessed: " + isAlreadyProcessed);

                                    // not process, charge credit card
                                    if (false == isAlreadyProcessed)
                                    {
                                        debug.AppendLine("Processing credit card...");

                                        debug.AppendLine("checkoutDetails: " + (checkoutDetails == null ? "NULL!" : "Not null"));
                                        debug.AppendLine("checkoutBilling: " + (checkoutBilling == null ? "NULL!" : "RegId: " + checkoutBilling.RegId));

                                        creditCardResult = cartsObj.ProcessCreditCard(checkoutDetails, checkoutBilling);

                                        debug.AppendLine("creditCardResult: " + (creditCardResult == null ? "NULL!" : "ErrorCode: " + creditCardResult.ErrorCode));

                                        isError = creditCardResult.ErrorCode != 0;
                                    }
                                }
                                else
                                {
                                    // check for duplicate
                                    debug.AppendLine("Checking for duplicate... (tempCust.reg_ID: "+ tempCust.reg_ID ?? 0 + ")");

                                    reg = Registrations.GetRegistrationByCartId(tempCust.reg_ID ?? 0);

                                    if (reg != null)
                                    {
                                        debug.AppendLine("Already processed!");

                                        isAlreadyProcessed = true;
                                    }
                                }

                                debug.AppendLine("Checking for Error...");
                                debug.AppendLine("isError: " + isError);

                                if (true == isError)
                                {
                                    debug.AppendLine(" - tempCust: " + (tempCust == null ? "NULL!" : "ID: " + tempCust.ID));
                                    debug.AppendLine(" - creditCardResult: " + (creditCardResult == null ? "NULL!" : "ErrorCode: " + creditCardResult.ErrorCode));

                                    cartsObj.AddToTempError(tempCust, creditCardResult);
                                    cartsObj.SendCreditCardErrorEmail(tempCust, creditCardResult);

                                    debug.AppendLine("Redirecting to: /register/info/?error=cc");

                                    return Redirect("/register/info/?error=cc");
                                }
                                else
                                {
                                    debug.AppendLine(" - isAlreadyProcessed (no error): " + isAlreadyProcessed);
                                    
                                    CartCookies.Remove();
                                    
                                    if (true == isAlreadyProcessed)
                                    {
                                        debug.AppendLine(" - reg: " + (reg == null ? "NULL!" : "ID: " + reg.RegistrationID));

                                        if (reg == null)
                                        {
                                            reg = Registrations.GetRegistrationByCartId(tempCust.reg_ID ?? 0);

                                            if (reg != null)
                                            {
                                                debug.AppendLine("Already processed!");

                                                isAlreadyProcessed = true;
                                            }
                                        }

                                        debug.AppendLine(" - reg: " + (reg == null ? "NULL!" : "ID: " + reg.RegistrationID));

                                        if (reg != null)
                                        {
                                            Session["RegistrationId"] = reg.RegistrationID;
                                            Session["RegistrationTotal"] = string.Format("{0:N2}", reg.RegOrderTotal ?? 0);

                                            debug.AppendLine("Already processed...");
                                            debug.AppendLine("reg.RegistrationID: " + reg.RegistrationID + ")");
                                            debug.AppendLine("reg.RegOrderTotal: " + reg.RegOrderTotal ?? 0 + ")");

                                            debug.AppendLine("Redirecting to: /register/success/");

                                            return Redirect("/register/success/");
                                        }
                                        else
                                        {
                                            debug.AppendLine("CAN'T FIND REGISTRATION!");

                                            cartsObj.SendCartErrorEmail("ERROR: 95\n\rCan't Find Registration: (reg == null || reg.RegistrationID <= 0)\r\n\r\nDebug:\r\n" + debug.ToString());

                                            return Redirect("/register/error/?error=95&regid=" + tempCust.reg_ID);
                                        }
                                    }
                                    else
                                    {
                                        // convert temp to registration
                                        debug.AppendLine("Convert temp to registration...");

                                        debug.AppendLine("tempCust.reg_ID: " + tempCust.reg_ID ?? 0 + ")");

                                        reg = Registrations.AddRegistrationByTempCart(tempCust.reg_ID ?? 0);

                                        debug.AppendLine("reg: " + reg == null ? "NULL!" : "RegistrationID: " + reg.RegistrationID);

                                        if (reg != null && reg.RegistrationID > 0)
                                        {
                                            Session["RegistrationId"] = reg.RegistrationID;
                                            Session["RegistrationTotal"] = string.Format("{0:N2}", reg.RegOrderTotal ?? 0);

                                            debug.AppendLine("reg.RegistrationID: " + reg.RegistrationID + ")");
                                            debug.AppendLine("reg.RegOrderTotal: " + reg.RegOrderTotal ?? 0 + ")");

                                            // Email Registrar and Billing
                                            debug.AppendLine("Email order confirmations...");
                                            Registrations.EmailOrderConfirmations(checkoutDetails, reg);

                                            // Email Attendees
                                            debug.AppendLine("Email attendee confirmations...");
                                            Registrations.EmailAttendeeConfirmations(checkoutDetails, reg);

                                            return Redirect("/register/success/");
                                        }
                                        else
                                        {
                                            // registration wasn't created
                                            cartsObj.SendCartErrorEmail("ERROR: 90\n\rregistration wasn't created: (reg == null || reg.RegistrationID <= 0)\r\n\r\nDebug:\r\n" + debug.ToString());

                                            int tempRegId = tempCust.reg_ID ?? 0;

                                            return Redirect("/register/error/?error=90&regid=" + tempRegId);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                cartsObj.SendCartErrorEmail("ERROR: 91\n\rregistration wasn't created: (tempCust == null || tempAttList == null || tempAttList.Count <= 0)\r\n\r\nDebug:\r\n" + debug.ToString());

                                return Redirect("/register/error/?error=91");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        cartsObj.SendCheckoutErrorEmail("RegID: " + tempRegList[0].reg_ID + "\n\rDebug:\r\n"+ debug.ToString() + "\r\n\r\nError: " + ex.ToString());
                    }
                }
                else
                {
                    cartsObj.SendCartErrorEmail("ERROR: 92\n\rregistration list null: (tempRegList == null)\r\n\r\nDebug:\r\n" + debug.ToString());

                    return Redirect("/register/error/?error=92");
                }

                cartsObj.SendCartErrorEmail("ERROR: 93\n\rfunction fall through.\r\n\r\nDebug:\r\n" + debug.ToString());

                return Redirect("/register/error/?error=93");
            }
        }
    }
}