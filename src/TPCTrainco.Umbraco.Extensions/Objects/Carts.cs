﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPCTrainco.Umbraco.Extensions.Models;
using TPCTrainco.Umbraco.Extensions.ViewModels;
using TPCTrainco.Umbraco.Extensions.Helpers;
using MoreLinq;
using System.Web;
using System.Runtime.Caching;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Net;
using System.IO;

namespace TPCTrainco.Umbraco.Extensions.Objects
{
    public class Carts
    {
        private string CacheCartKey = "CartGuid:";

        public CartResponse SaveCartItems(List<CartItem> cartItemList)
        {
            CartResponse response = new CartResponse();
            int cartRegID = 0;  // reg_ID = CART_ID

            // Check cart values
            if (cartItemList == null || cartItemList.Count <= 0)
            {
                response.Success = false;
                response.Message = "Cart is empty.";

                return response;
            }
            else
            {
                bool foundIssue = false;
                string issueMessage = null;

                foreach (CartItem cartItem in cartItemList)
                {
                    // check schedule
                    if (cartItem.Id > 0)
                    {
                        using (var db = new ATI_DevelopmentEntities1())
                        {
                            SCHEDULE schedule = db.SCHEDULES.Where(p => p.ScheduleID == cartItem.Id).FirstOrDefault();

                            if (schedule == null)
                            {
                                foundIssue = true;
                                issueMessage = "Schedule id not found.";
                                break;
                            }
                        }
                    }
                    else
                    {
                        foundIssue = true;
                        issueMessage = "Invalid schedule id.";
                        break;
                    }

                    // check quantity
                    if (cartItem.Quantity <= 0)
                    {
                        foundIssue = true;
                        issueMessage = "Invalid quanity number.";
                        break;
                    }
                }

                if (true == foundIssue)
                {
                    response.Success = false;
                    response.Message = issueMessage;

                    return response;
                }
            }

            // Create new cart
            List<City> cityList = null;
            List<State> stateList = null;
            short seminarNum = 0;
            using (var db = new ATI_DevelopmentEntities1())
            {
                cityList = db.Set<City>().ToList();
                stateList = db.Set<State>().ToList();
            }

            // Get Cart ID (reg_ID)


            using (var db = new ATI_DevelopmentEntities1())
            {
                cartRegID = db.temp_Reg.Max(p => p.reg_ID);

                cartRegID++;
            }

            // loop through cart
            foreach (CartItem cartItem in cartItemList)
            {
                seminarNum++;

                using (var db = new ATI_DevelopmentEntities1())
                {
                    temp_Reg tempReg = new temp_Reg();

                    string seminarTitle = null;
                    string seminarPlace = null;
                    string seminarFeeName = null;

                    SCHEDULE schedule = db.SCHEDULES.Where(p => p.ScheduleID == cartItem.Id).FirstOrDefault();

                    if (schedule != null)
                    {
                        City city = cityList.Where(p => p.CityID == schedule.CityID).FirstOrDefault();
                        State state = stateList.Where(p => p.StateID == schedule.StateID).FirstOrDefault();

                        ScheduleCourseInstructor scheduleCourseInstructor = db.ScheduleCourseInstructors.Where(p => p.ScheduleID == schedule.ScheduleID).FirstOrDefault();

                        if (scheduleCourseInstructor != null)
                        {
                            COURS course = db.COURSES.Where(p => p.CourseID == scheduleCourseInstructor.CourseID).FirstOrDefault();

                            if (course != null)
                            {
                                CourseFormat courseFormat = db.CourseFormats.Where(p => p.CourseFormatID == course.CourseFormatID).FirstOrDefault();

                                if (courseFormat != null)
                                {
                                    seminarTitle = course.TitlePlain;

                                    decimal courseFee = scheduleCourseInstructor.CourseFee ?? 0;

                                    seminarFeeName = courseFormat.CourseFormatName + " - $" + courseFee.ToString("N0");

                                    seminarPlace = city.CityName + ", " + state.StateAbbreviation + " - " + schedule.ScheduleDateDescription;

                                    // Create temp_Reg object
                                    tempReg.reg_ID = cartRegID;
                                    tempReg.reg_Date = DateTime.Now;
                                    tempReg.sem_No = seminarNum;
                                    tempReg.sem_SID = cartItem.Id;
                                    tempReg.sem_Title = seminarTitle;
                                    tempReg.sem_Place = seminarPlace;
                                    tempReg.sem_FeeName = seminarFeeName;
                                    tempReg.sem_FeeAmt = courseFee;
                                    tempReg.sem_Qty = cartItem.Quantity;

                                    db.temp_Reg.Add(tempReg);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }

            response.Success = true;
            response.Message = "Success!";
            response.CartGuid = Guid.NewGuid().ToString().ToLower();

            RefreshCartCache(response.CartGuid, cartRegID);

            return response;
        }


        public CheckoutResponse SaveCheckoutItems(CheckoutCart checkoutCart)
        {
            CheckoutResponse response = new CheckoutResponse();

            response.Success = true;
            response.InvalidItems = new List<CheckoutItemResponse>();

            if (checkoutCart != null && checkoutCart.CheckoutItems != null && checkoutCart.CheckoutItems.Count > 0)
            {
                response.CartGuid = checkoutCart.CartGuid;

                foreach (CheckoutItem checkoutItem in checkoutCart.CheckoutItems)
                {
                    var valContext = new ValidationContext(checkoutItem, serviceProvider: null, items: null);
                    var validationResultList = new List<ValidationResult>();

                    bool isValid = Validator.TryValidateObject(checkoutItem, valContext, validationResultList);

                    if (false == isValid)
                    {
                        response.Success = false;
                        response.Message = "There was a problem with the form. Please verifiy each field and try again.";

                        foreach (ValidationResult validationResult in validationResultList)
                        {
                            CheckoutItemResponse itemResponse = new CheckoutItemResponse();
                            itemResponse.ElementId = "";

                            if (validationResult.MemberNames != null && validationResult.MemberNames.Count() > 0)
                            {
                                itemResponse.ElementId = validationResult.MemberNames.First().ToLower() + checkoutItem.AttendeeInc;
                            }

                            itemResponse.Message = validationResult.ErrorMessage;
                            response.InvalidItems.Add(itemResponse);
                        }
                    }
                    else
                    {
                        if (false == IsValidEmailAddress(checkoutItem.Email))
                        {
                            CheckoutItemResponse itemResponse = new CheckoutItemResponse();

                            itemResponse.ElementId = "email" + checkoutItem.AttendeeInc;

                            itemResponse.Message = "Invalid email address.";
                            response.InvalidItems.Add(itemResponse);
                        }
                    }
                }
            }
            else
            {
                response.Success = false;
                response.Message = "Invalid cart.";
            }

            if (true == response.Success)
            {
                List<temp_Reg> cart = GetCart(response.CartGuid);

                if (cart != null && cart.Count > 0)
                {
                    if (checkoutCart != null && checkoutCart.CheckoutItems != null && checkoutCart.CheckoutItems.Count > 0)
                    {
                        using (var db = new ATI_DevelopmentEntities1())
                        {
                            foreach (temp_Reg tempReg in cart)
                            {
                                List<CheckoutItem> checkoutItemList = checkoutCart.CheckoutItems.Where(p => p.SeminarId == tempReg.sem_SID).ToList();

                                if (checkoutItemList != null && checkoutItemList.Count > 0)
                                {
                                    foreach (CheckoutItem checkoutItem in checkoutItemList)
                                    {
                                        temp_Att tempAtt = new temp_Att();

                                        tempAtt.reg_SEQ = tempReg.reg_SEQ;
                                        tempAtt.att_No = checkoutItem.AttendeeNum;
                                        tempAtt.att_FName = checkoutItem.FirstName;
                                        tempAtt.att_LName = checkoutItem.LastName;
                                        tempAtt.att_Title = checkoutItem.Title;
                                        tempAtt.att_Email = checkoutItem.Email;
                                        tempAtt.reg_ID = tempReg.reg_ID;
                                        tempAtt.att_MName = "";
                                        tempAtt.att_Suffix = "";

                                        db.temp_Att.Add(tempAtt);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
            }


            return response;
        }


        public temp_Cust ConvertModelToTempCust(CheckoutCustomer checkoutCust, List<temp_Reg> tempReg)
        {
            temp_Cust tempCust = null;

            if (checkoutCust != null && tempReg != null && tempReg.Count > 0)
            {
                tempCust = new temp_Cust();

                tempCust.reg_ID = tempReg[0].reg_ID;

                decimal feeTotal = tempReg.Sum(p => (p.sem_FeeAmt ?? 0) * p.sem_Qty ?? 0);

                tempCust.reg_Cost = Convert.ToInt32(feeTotal);
                tempCust.promoCode = checkoutCust.PromoCode;
                tempCust.promoType = checkoutCust.HearAbout;
                tempCust.promoDesc = checkoutCust.HearAboutOther;
                tempCust.CoName = checkoutCust.Company;
                tempCust.authFName = checkoutCust.FirstName;
                tempCust.authLName = checkoutCust.LastName;
                tempCust.authTitle = checkoutCust.Title;
                tempCust.authMailCode = "";
                tempCust.authAddr1 = checkoutCust.Address;
                tempCust.authAddr2 = checkoutCust.Address2;
                tempCust.authCity = checkoutCust.City;
                tempCust.authState = checkoutCust.State;
                tempCust.authZip = checkoutCust.Zip;
                tempCust.authCountry = checkoutCust.Country;

                List<string> phoneArray = StringUtilities.SplitPhoneNumber(checkoutCust.Phone);
                tempCust.authPhone1 = phoneArray[0];
                tempCust.authPhone2 = phoneArray[1];
                tempCust.authPhone3 = phoneArray[2];

                tempCust.authPhoneExt = checkoutCust.PhoneExt;
                tempCust.authFax1 = "";
                tempCust.authFax2 = "";
                tempCust.authFax3 = "";
                tempCust.authEmail = checkoutCust.Email;

                if (true == checkoutCust.BillingDifferent)
                {
                    tempCust.billFName = checkoutCust.BillFirstName;
                    tempCust.billLName = checkoutCust.BillLastName;
                    tempCust.billTitle = "";
                    tempCust.billMailCode = "";
                    tempCust.billAddr1 = checkoutCust.BillAddress;
                    tempCust.billAddr2 = checkoutCust.BillAddress2;
                    tempCust.billCity = checkoutCust.BillCity;
                    tempCust.billState = checkoutCust.BillState;
                    tempCust.billZip = checkoutCust.BillZip;
                    tempCust.billCountry = checkoutCust.BillCountry;
                    tempCust.billPhone1 = tempCust.authPhone1;
                    tempCust.billPhone2 = tempCust.authPhone2;
                    tempCust.billPhone3 = tempCust.authPhone3;
                    tempCust.billPhoneExt = tempCust.authPhoneExt;
                    tempCust.billFax1 = "";
                    tempCust.billFax2 = "";
                    tempCust.billFax3 = "";
                    tempCust.billEmail = tempCust.authEmail;
                }
                else
                {
                    tempCust.billFName = tempCust.authFName;
                    tempCust.billLName = tempCust.authLName;
                    tempCust.billTitle = tempCust.authTitle;
                    tempCust.billMailCode = "";
                    tempCust.billAddr1 = tempCust.authAddr1;
                    tempCust.billAddr2 = tempCust.authAddr2;
                    tempCust.billCity = tempCust.authCity;
                    tempCust.billState = tempCust.authState;
                    tempCust.billZip = tempCust.authZip;
                    tempCust.billCountry = tempCust.authCountry;
                    tempCust.billPhone1 = tempCust.authPhone1;
                    tempCust.billPhone2 = tempCust.authPhone2;
                    tempCust.billPhone3 = tempCust.authPhone3;
                    tempCust.billPhoneExt = tempCust.authPhoneExt;
                    tempCust.billFax1 = "";
                    tempCust.billFax2 = "";
                    tempCust.billFax3 = "";
                    tempCust.billEmail = tempCust.authEmail;
                }
                tempCust.ConfTo = "";

                string paymentMethod = checkoutCust.PaymentType;
                if (paymentMethod == "credit")
                {
                    paymentMethod = "Credit Card";
                }
                else if (paymentMethod == "invoice")
                {
                    paymentMethod = "Invoice Me";
                }
                tempCust.payMethod = paymentMethod;

                if (tempCust.payMethod == "Credit Card")
                {
                    string ccType = StringUtilities.CreditCardType(checkoutCust.CCNumber);
                    tempCust.ccType = ccType;

                    tempCust.ccName = checkoutCust.CCName;
                    tempCust.ccNumber = StringUtilities.GetLast(checkoutCust.CCNumber, 4);

                    DateTime dtExpire = StringUtilities.GetExpirationDate(checkoutCust.CCExpiration);
                    tempCust.ccMonth = dtExpire.Month;
                    tempCust.ccYear = dtExpire.Year;
                    tempCust.ccCVC = checkoutCust.CVVCode;
                }
                else
                {
                    tempCust.ccType = "";
                    tempCust.ccName = "";
                    tempCust.ccNumber = "";
                    tempCust.ccMonth = DateTime.Now.Month;
                    tempCust.ccYear = DateTime.Now.Year;
                    tempCust.ccCVC = "";

                }
                tempCust.chkNo = "";
                tempCust.poNo = checkoutCust.PONumber;
            }

            return tempCust;
        }


        public CheckoutCustomer ConvertTempCustToCheckoutCustomer(temp_Cust tempCust)
        {
            CheckoutCustomer checkoutCust = null;

            if (tempCust != null)
            {
                checkoutCust = new CheckoutCustomer();

                checkoutCust.PromoCode = tempCust.promoCode;
                checkoutCust.HearAbout = tempCust.promoType;
                checkoutCust.HearAboutOther = tempCust.promoDesc;
                checkoutCust.Company = tempCust.CoName;
                checkoutCust.FirstName = tempCust.authFName;
                checkoutCust.LastName = tempCust.authLName;
                checkoutCust.Title = tempCust.authTitle;
                checkoutCust.Address = tempCust.authAddr1;
                checkoutCust.Address2 = tempCust.authAddr2;
                checkoutCust.City = tempCust.authCity;
                checkoutCust.State = tempCust.authState;
                checkoutCust.Zip = tempCust.authZip;
                checkoutCust.Country = tempCust.authCountry;

                checkoutCust.Phone = tempCust.authPhone1 + "-" + tempCust.authPhone2 + "-" + tempCust.authPhone3;
                checkoutCust.PhoneExt = tempCust.authPhoneExt;
                checkoutCust.Email = tempCust.authEmail;
                checkoutCust.BillFirstName = tempCust.billFName;
                checkoutCust.BillLastName = tempCust.billLName;
                checkoutCust.BillAddress = tempCust.billAddr1;
                checkoutCust.BillAddress2 = tempCust.billAddr2;
                checkoutCust.BillCity = tempCust.billCity;
                checkoutCust.BillState = tempCust.billState;
                checkoutCust.BillZip = tempCust.billZip;
                checkoutCust.BillCountry = tempCust.billCountry;

                string paymentMethod = tempCust.payMethod;
                if (paymentMethod == "Credit Card")
                {
                    paymentMethod = "credit";
                }
                else if (paymentMethod == "Invoice Me")
                {
                    paymentMethod = "invoice";
                }
                checkoutCust.PaymentType = paymentMethod;

                checkoutCust.CCName = tempCust.ccName;

                DateTime dtExpire = DateTime.Parse(tempCust.ccMonth + "/" + tempCust.ccYear);
                checkoutCust.CCExpiration = dtExpire.ToString("MM/yy");
                checkoutCust.CVVCode = tempCust.ccCVC;
            }

            return checkoutCust;
        }


        public temp_Cust SaveCheckoutDetails(temp_Cust tempCust)
        {
            temp_Cust response = null;

            using (var db = new ATI_DevelopmentEntities1())
            {
                db.temp_Cust.Add(tempCust);
                db.SaveChanges();

                response = tempCust;
            }

            return response;
        }


        public bool CreditCardAlreadyProcessed(temp_Cust tempCust)
        {
            bool isAlreadyProcessed = false;
            CC_Log ccLog = null;

            using (var db = new ATI_DevelopmentEntities1())
            {
                // SELECT COUNT(*) FROM CC_Log WHERE ProcessedFrom <> 'MartinEPA' AND ProcessedFrom <> 'Filemaker' AND Approval = 1 AND Cart_ID = " & cartID.ToString()

                ccLog = db.CC_Log.Where(p => p.ProcessedFrom != "MartinEPA" && p.ProcessedFrom != "Filemaker" && p.Approval == 1 && p.Cart_ID == tempCust.reg_ID).FirstOrDefault();

                if (ccLog != null)
                {
                    isAlreadyProcessed = true;
                }
            }

            return isAlreadyProcessed;
        }


        public CreditCardResult ProcessCreditCard(CheckoutDetails checkout, CheckoutBilling billing)
        {
            CreditCardResult creditCardResult = new CreditCardResult();

            int errorCode = 999;
            string errorText = null;

            creditCardResult.ErrorCode = errorCode;
            creditCardResult.ErrorText = null;

            bool websiteTestMode = ConfigurationManager.AppSettings.Get("CC2:WebsiteTestMode") == "1" ? true : false;
            string ccCurrency = ConfigurationManager.AppSettings.Get("CC2:Currency");
            string ccMerchantId = ConfigurationManager.AppSettings.Get("CC2:MerchID");
            string ccUserId = ConfigurationManager.AppSettings.Get("CC2:UserID");
            string ccPin = ConfigurationManager.AppSettings.Get("CC2:PIN");
            string ccTest = ConfigurationManager.AppSettings.Get("CC2:IsTest");

            if (false == websiteTestMode)
            {

                int ccMonth = checkout.tempCust.ccMonth ?? 1;
                int ccYear = checkout.tempCust.ccYear ?? 2000;

                DateTime dtExpire = DateTime.Parse(ccMonth + "/1/" + ccYear);
                string description = checkout.tempCust.CoName;
                description += "_" + checkout.tempCust.authPhone1 + "-" + checkout.tempCust.authPhone2 + "-" + checkout.tempCust.authPhone3 + "_";
                description += checkout.tempCust.authLName + "_" + checkout.tempCust.authFName;


                string phone = "(" + checkout.tempCust.billPhone1 + ") " + checkout.tempCust.billPhone2 + "-" + checkout.tempCust.billPhone3;
                string fax = "";

                if (false == string.IsNullOrWhiteSpace(checkout.tempCust.billFax1))
                {
                    fax = "(" + checkout.tempCust.billFax1 + ") " + checkout.tempCust.billFax2 + "-" + checkout.tempCust.billFax3;
                }

                decimal orderTotal = Convert.ToDecimal(checkout.tempCust.reg_Cost ?? 0);
                string orderTotalStr = String.Format("{0:C}", orderTotal).Replace("$", "").Replace(",", "");

                string processorUrl = "https://www.myvirtualmerchant.com/VirtualMerchant/process.do";
                string results = null;

                string postData = "ssl_transaction_type=CCSALE";
                postData += "&ssl_merchant_id=" + ccMerchantId;
                postData += "&ssl_pin=" + ccPin;
                postData += "&ssl_user_id=" + ccUserId;
                postData += "&ssl_test_mode=" + ccTest;

                postData += "&ssl_amount=" + orderTotalStr;
                postData += "&ssl_card_number=" + billing.CCNumber;
                postData += "&ssl_exp_date=" + dtExpire.ToString("MMyy");
                postData += "&ssl_cvv2cvc2_indicator=1";
                postData += "&ssl_cvv2cvc2=" + checkout.tempCust.ccCVC;

                postData += "&ssl_description=" + StringUtilities.StringMaxLength(description, 255); ;
                postData += "&ssl_invoice_number=" + checkout.tempCust.reg_ID;

                string customerCode = description;
                if (customerCode.Length > 17)
                {
                    customerCode = StringUtilities.StringMaxLength(customerCode.Replace("-", ""), 17);
                }
                postData += "&ssl_customer_code=" + customerCode;
                postData += "&ssl_company=" + StringUtilities.StringMaxLength(checkout.tempCust.CoName, 50);
                postData += "&ssl_first_name=" + StringUtilities.StringMaxLength(checkout.tempCust.billFName, 20);
                postData += "&ssl_last_name=" + StringUtilities.StringMaxLength(checkout.tempCust.billLName, 30);
                postData += "&ssl_avs_address=" + StringUtilities.StringMaxLength(checkout.tempCust.billAddr1, 20);
                postData += "&ssl_city=" + StringUtilities.StringMaxLength(checkout.tempCust.billCity, 30);
                postData += "&ssl_state=" + StringUtilities.StringMaxLength(checkout.tempCust.billState, 50);
                postData += "&ssl_avs_zip=" + StringUtilities.StringMaxLength(checkout.tempCust.billZip, 9);
                postData += "&ssl_country=" + StringUtilities.StringMaxLength(checkout.tempCust.billCountry, 30);
                postData += "&ssl_phone=" + StringUtilities.StringMaxLength(phone, 20);
                postData += "&ssl_email=" + StringUtilities.StringMaxLength(checkout.tempCust.billEmail, 100);

                postData += "&ssl_show_form=FALSE";
                postData += "&ssl_result_format=ASCII";

                byte[] data = Encoding.ASCII.GetBytes(postData);
                WebRequest request = WebRequest.Create(processorUrl);
                request.Method = "POST";
                request.ContentLength = data.Length;
                request.ContentType = "application/x-www-form-urlencoded";

                try
                {
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    HttpWebResponse objResponse = (HttpWebResponse)request.GetResponse();
                    string responseString = new StreamReader(objResponse.GetResponseStream()).ReadToEnd();

                    List<string> responseArray = responseString.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    foreach (string returnVal in responseArray)
                    {
                        List<string> returnValArray = returnVal.Split('=').ToList();

                        if (returnValArray.Count == 2)
                        {
                            if (returnValArray[0].IndexOf("ssl_result") >= 0)
                            {
                                errorCode = Convert.ToInt32(returnValArray[1]);
                            }
                            else if (returnValArray[0].IndexOf("ssl_result_message") >= 0)
                            {
                                errorText = returnValArray[1];
                            }
                            else if (returnValArray[0].IndexOf("errorCode") >= 0)
                            {
                                errorCode = Convert.ToInt32(returnValArray[1]);
                            }
                            else if (returnValArray[0].IndexOf("ssl_result_message") >= 0)
                            {
                                errorText = returnValArray[1];
                            }

                            // for log
                            results += returnVal;
                        }
                    }

                    if (errorCode == 0)
                    {
                        errorText = "";
                    }

                    AddToCCLog(checkout.tempCust, results);

                }
                catch (Exception ex)
                {
                    errorCode = 900;
                }
            }
            else // websiteTestMode ON
            {
                errorCode = 0;
            }

            creditCardResult.ErrorCode = errorCode;
            creditCardResult.ErrorText = errorText;

            return creditCardResult;
        }


        public void AddToCCLog(temp_Cust tempCust, string result)
        {
            CC_Log ccLog = new CC_Log();

            ccLog.Cart_ID = tempCust.reg_ID;
            ccLog.Reg_TR_Number = 0;
            ccLog.CC_Type = tempCust.ccType;
            ccLog.CC_Num = tempCust.ccNumber;
            ccLog.CC_Amount = Convert.ToDecimal(tempCust.reg_Cost ?? 0);
            ccLog.CC_Result = result;
            ccLog.ProcessedFrom = "ATI";

            using (var db = new ATI_DevelopmentEntities1())
            {
                db.CC_Log.Add(ccLog);
                db.SaveChanges();
            }
        }


        public void AddToTempError(temp_Cust tempCust, CreditCardResult result)
        {
            temp_Errors tempError = new temp_Errors();

            tempError.errDate = DateTime.Now;
            tempError.tempRegID = tempCust.reg_ID;
            tempError.attFName = "CC Process Error";
            tempError.attLName = StringUtilities.StringMaxLength(result.ErrorText, 255);

            using (var db = new ATI_DevelopmentEntities1())
            {
                db.temp_Errors.Add(tempError);
                db.SaveChanges();
            }
        }


        public void SendCreditCartErrorEmail(temp_Cust tempCust, CreditCardResult result)
        {
            List<string> emailToList = null;

            if (ConfigurationManager.AppSettings["LogToEmail:CCError"] != null && ConfigurationManager.AppSettings.Get("LogToEmail:CCError").Length > 0)
            {
                emailToList = new List<string>();

                emailToList = ConfigurationManager.AppSettings.Get("LogToEmail:CCError").Split(';').ToList();

                Helpers.Email email = new Email();

                email.EmailFrom = "website@americantrainco.com";
                email.EmailToList = emailToList;
                email.Subject = "TPCTrainco.com Credit Card Error";
                email.IsBodyHtml = false;

                string body = "=- TPCTrainco.com Credit Card Error =-" + Environment.NewLine + Environment.NewLine;
                body += "Temp Cart ID: " + tempCust.reg_ID + Environment.NewLine;
                body += "First Name: " + tempCust.authFName + Environment.NewLine;
                body += "Last Name: " + tempCust.authLName + Environment.NewLine;
                body += "CC Error Code: " + result.ErrorCode + Environment.NewLine;
                body += "CC Error Text: " + result.ErrorText + Environment.NewLine;

                email.Body = body;

                email.SendEmail();
            }
        }


        public void SendCheckoutErrorEmail(string emailBody)
        {
            List<string> emailToList = null;

            if (ConfigurationManager.AppSettings["LogToEmail:CCError"] != null && ConfigurationManager.AppSettings.Get("LogToEmail:CCError").Length > 0)
            {
                emailToList = new List<string>();

                emailToList = ConfigurationManager.AppSettings.Get("LogToEmail:CCError").Split(';').ToList();

                Helpers.Email email = new Email();

                email.EmailFrom = "website@americantrainco.com";
                email.EmailToList = emailToList;
                email.Subject = "TPCTrainco.com Registration Error";
                email.IsBodyHtml = false;

                string body = emailBody

                email.Body = body;

                email.SendEmail();
            }
        }


        public bool DeleteTempAtt(int regId)
        {
            bool success = false;

            using (var db = new ATI_DevelopmentEntities1())
            {
                db.temp_Att.RemoveRange(db.temp_Att.Where(p => p.reg_ID == regId));
                db.SaveChanges();

                success = true;
            }

            return success;
        }


        public bool DeleteTempCust(int regId)
        {
            bool success = false;

            using (var db = new ATI_DevelopmentEntities1())
            {
                temp_Cust deleteTempCust = db.temp_Cust.Where(p => p.reg_ID == regId).FirstOrDefault();

                if (deleteTempCust != null)
                {
                    db.temp_Cust.Remove(deleteTempCust);
                    db.SaveChanges();
                }

                success = true;
            }

            return success;
        }


        public List<temp_Reg> GetCart(string cartGuid)
        {
            List<temp_Reg> cart = null;

            if (false == string.IsNullOrEmpty(cartGuid))
            {
                int regId = GetCartCache(cartGuid);

                if (regId > 0)
                {
                    using (var db = new ATI_DevelopmentEntities1())
                    {
                        cart = db.temp_Reg.Where(p => p.reg_ID == regId).ToList();
                    }
                }
            }

            return cart;
        }



        private int GetCartCache(string cartGuid)
        {
            object regIdObj = null;
            string regIdStr = null;
            int regId = 0;

            ObjectCache cache = MemoryCache.Default;

            regIdObj = cache.Get(CacheCartKey + cartGuid);

            regIdStr = regIdObj != null ? regIdObj.ToString() : null;

            if (false == string.IsNullOrEmpty(regIdStr))
            {
                regId = Convert.ToInt32(regIdStr);
            }

            return regId;
        }



        private void RefreshCartCache(string cartGuid, int regId)
        {
            string cacheKey = CacheCartKey + cartGuid;
            int cacheUpdateInMinutes = 10;
            ObjectCache cache = MemoryCache.Default;

            CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
            cache.Add(cacheKey, regId, policy);
        }



        private static bool IsValidEmailAddress(string emailAddress)
        {
            return new System.ComponentModel.DataAnnotations
                                .EmailAddressAttribute()
                                .IsValid(emailAddress);
        }
    }
}
