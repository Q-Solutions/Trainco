using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;
using TPCTrainco.Umbraco.Extensions;
using TPCTrainco.Umbraco.Extensions.Helpers;
using TPCTrainco.Umbraco.Extensions.Models;
using TPCTrainco.Umbraco.Extensions.Models.Account;
using TPCTrainco.Umbraco.Extensions.Objects;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;

namespace TPCTrainco.Umbraco.Controllers
{
    public class CheckoutCustomerSurfaceController : SurfaceController
    {
        public ActionResult Index()
        {
            CheckoutCustomer checkoutCustomer = new CheckoutCustomer();
            try
            {
                List<temp_Reg> cartList = null;
                string cartGuid = null;
                string tokenKey = null;

                cartGuid = Carts.GetCartGuid(Session);
                tokenKey = Users.GetToken(Session);

                if (false == string.IsNullOrEmpty(tokenKey))
                {
                    string memberKey = AccountHelper.GetMemberKeyFromToken(tokenKey);

                    UserModel user = null;
                    CompanyModel company = null;
                    BillingModel billing = null;

                    if (false == string.IsNullOrEmpty(memberKey))
                    {
                        user = AccountHelper.GetUser(memberKey);
                        company = AccountHelper.GetCompany(memberKey);
                        billing = AccountHelper.GetBilling(memberKey);
                    }

                    if (user != null && company != null)
                    {
                        checkoutCustomer.LoggedIn = true;
                        checkoutCustomer.UserToken = tokenKey;

                        checkoutCustomer.FirstName = user.FirstName;
                        checkoutCustomer.LastName = user.LastName;
                        checkoutCustomer.Email = user.Email;
                        checkoutCustomer.Title = user.Title;

                        checkoutCustomer.Phone = user.Phone;
                        checkoutCustomer.PhoneExt = user.PhoneExtension;
                        checkoutCustomer.BillPhone = user.Phone;
                        checkoutCustomer.BillPhoneExt = user.PhoneExtension;

                        checkoutCustomer.Company = company.Name;
                        checkoutCustomer.Address = company.Address1;
                        checkoutCustomer.Address2 = company.Address2;
                        checkoutCustomer.City = company.City;
                        checkoutCustomer.State = company.State;

                        if (false == string.IsNullOrEmpty(company.State) && company.State.Length > 2)
                        {
                            State stateCode = CacheObjects.GetStateList().Where(p => p.StateName.ToLower() == company.State.ToLower()).FirstOrDefault();

                            if (stateCode != null)
                            {
                                checkoutCustomer.State = stateCode.StateAbbreviation;
                            }

                        }

                        checkoutCustomer.Zip = company.PostalCode;
                        checkoutCustomer.Country = company.Country;

                        if (billing != null && false == string.IsNullOrEmpty(billing.CompanyName) && false == string.IsNullOrEmpty(billing.Address1))
                        {
                            checkoutCustomer.SavedCompanyBilling = true;

                            checkoutCustomer.BillFirstName = billing.FirstName;
                            checkoutCustomer.BillLastName = billing.LastName;
                            checkoutCustomer.BillEmail = billing.Email;
                            checkoutCustomer.BillAddress = billing.Address1;
                            checkoutCustomer.BillAddress2 = billing.Address2;
                            checkoutCustomer.BillCity = billing.City;
                            checkoutCustomer.BillState = billing.State;

                            if (false == string.IsNullOrEmpty(billing.State) && billing.State.Length > 2)
                            {
                                State stateCode = CacheObjects.GetStateList().Where(p => p.StateName.ToLower() == billing.State.ToLower()).FirstOrDefault();

                                if (stateCode != null)
                                {
                                    checkoutCustomer.BillState = stateCode.StateAbbreviation;
                                }

                            }

                            checkoutCustomer.BillZip = billing.PostalCode;
                            checkoutCustomer.BillCountry = billing.Country;
                        }

                    }
                    else
                    {

                    }
                }


                if (false == string.IsNullOrWhiteSpace(cartGuid))
                {
                    Carts cartsObj = new Carts();

                    cartList = cartsObj.GetCart(cartGuid);

                    if (cartList == null)
                    {
                        checkoutCustomer.Redirect = "/register/?cart=" + Server.UrlEncode(cartGuid);
                    }
                    else
                    {
                        Session["CartId"] = cartGuid;

                        temp_Cust tempCust = null;

                        using (var db = new americantraincoEntities())
                        {
                            int regId = cartList[0].reg_ID;
                            checkoutCustomer.RegId = regId;

                            tempCust = db.temp_Cust.Where(p => p.reg_ID == regId).FirstOrDefault();

                            if (tempCust != null)
                            {
                                checkoutCustomer = cartsObj.ConvertTempCustToCheckoutCustomer(tempCust);
                                checkoutCustomer.BillingDifferent = true;
                            }
                        }

                        checkoutCustomer.CartGuid = CartCookies.EncryptCartGuid(cartGuid + "|" + checkoutCustomer.RegId + "|" + Request.UserHostAddress);

                        if (false == cartsObj.IsValidCart(checkoutCustomer.RegId, "/register/info/"))
                        {
                            checkoutCustomer.Redirect = "/register/?cart=" + cartGuid;
                        }
                    }
                }
                else
                {
                    checkoutCustomer.Redirect = "/";
                }
            }
            catch (Exception ex)
            {
                checkoutCustomer.Redirect = "/register/error/";
            }
            return PartialView("CheckoutCustomer", checkoutCustomer);
        }


        [NotChildAction]
        [HttpPost]
        public ActionResult HandleFormSubmit(CheckoutCustomer model)
        {
            try
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
                    string tokenKey = null;
                    UserModel user = null;
                    CompanyModel company = null;
                    BillingModel billing = null;
                    string memberKey = null;

                    tokenKey = Users.GetToken(Session);
                    cartGuid = Carts.GetCartGuid(Session);

                    if (false == string.IsNullOrEmpty(tokenKey))
                    {
                        memberKey = AccountHelper.GetMemberKeyFromToken(tokenKey);

                        if (false == string.IsNullOrEmpty(memberKey))
                        {
                            user = AccountHelper.GetUser(memberKey);
                            company = AccountHelper.GetCompany(memberKey);
                            billing = AccountHelper.GetBilling(memberKey);
                        }
                    }

                    if (true == string.IsNullOrWhiteSpace(cartGuid) || model.RegId <= 0)
                    {
                        string cartGuidStr = CartCookies.DecryptCartGuid(model.CartGuid);

                        if (cartGuidStr.IndexOf("|") >= 0)
                        {
                            string[] cartGuidArray = cartGuidStr.Split('|');

                            if (cartGuidArray[2] == Request.UserHostAddress)
                            {
                                cartGuid = cartGuidArray[0];
                                model.RegId = Convert.ToInt32(cartGuidArray[1]);
                            }
                        }
                    }

                    if (false == string.IsNullOrWhiteSpace(cartGuid))
                    {
                        cartList = cartsObj.GetCart(cartGuid, model.RegId);

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
                        if (false == cartsObj.IsValidCart(cartList[0].reg_ID, "/register/info/ (POST)"))
                        {
                            return Redirect("/register/?cart=" + cartGuid);
                        }

                        checkoutBilling.RegId = cartList[0].reg_ID;

                        if (model.PaymentType == "credit")
                        {
                            checkoutBilling.CCNumber = model.CCNumber.Replace("-", "");
                        }

                        if (user != null && false == string.IsNullOrEmpty(user.Email))
                        {
                            model.Email = user.Email;
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
                                // If user is logged in, check for update company profile flag
                                if (false == string.IsNullOrEmpty(tokenKey) && false == string.IsNullOrEmpty(memberKey))
                                {
                                    if (true == model.UpdateCompanyProfile && user != null && company != null)
                                    {
                                        user.FirstName = model.FirstName;
                                        user.LastName = model.LastName;
                                        user.Title = model.Title;

                                        user.Phone = model.Phone;
                                        user.PhoneExtension = model.PhoneExt;

                                        company.Name = model.Company;
                                        company.Address1 = model.Address;
                                        company.Address2 = model.Address2;
                                        company.City = model.City;
                                        company.State = model.State;
                                        company.PostalCode = model.Zip;
                                        company.Country = model.Country;

                                        IMember umbracoMember = null;
                                        AccountHelper.UpdateUser(memberKey, user, out umbracoMember);
                                        AccountHelper.UpdateCompany(memberKey, company);

                                        if (true == model.UpdateCompanyBilling)
                                        {
                                            if (billing == null)
                                            {
                                                billing = new BillingModel();

                                                billing.FirstName = model.BillFirstName;
                                                billing.LastName = model.BillLastName;
                                                billing.Email = model.BillEmail;
                                                billing.Address1 = model.BillAddress;
                                                billing.Address2 = model.BillAddress2;
                                                billing.City = model.BillCity;
                                                billing.State = model.BillState;
                                                billing.PostalCode = model.BillZip;
                                                billing.Country = model.BillCountry;

                                                AccountHelper.UpdateBilling(memberKey, billing);
                                            }
                                        }
                                    }
                                }


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
            catch (Exception ex)
            {
                return Redirect("/register/error/");
            }
        }
    }
}