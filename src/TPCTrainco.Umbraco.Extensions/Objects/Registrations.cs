using MoreLinq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TPCTrainco.Umbraco.Extensions.Helpers;
using TPCTrainco.Umbraco.Extensions.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace TPCTrainco.Umbraco.Extensions.Objects
{
    public class Registrations
    {
        public static REGISTRATION GetRegistrationByCartId(int cartId)
        {
            REGISTRATION reg = null;

            if (cartId > 0)
            {
                using (var db = new americantraincoEntities())
                {
                    reg = db.REGISTRATIONS.Where(p => p.CartID == cartId).FirstOrDefault();
                }
            }

            return reg;
        }


        public static REGISTRATION GetRegistrationById(int registrationId)
        {
            REGISTRATION reg = null;

            if (registrationId > 0)
            {
                using (var db = new americantraincoEntities())
                {
                    reg = db.REGISTRATIONS.Where(p => p.RegistrationID == registrationId).FirstOrDefault();
                }
            }

            return reg;
        }

        public static REGISTRATION AddRegistrationByTempCart(int regId)
        {
            REGISTRATION reg = null;

            if (regId > 0)
            {
                using (var db = new americantraincoEntities())
                {
                    int? registraionId = db.add_Registration(regId).SingleOrDefault();

                    if (registraionId != null && registraionId > 0)
                    {
                        reg = GetRegistrationById(registraionId ?? 0);
                    }

                    reg = db.REGISTRATIONS.Where(p => p.CartID == regId).FirstOrDefault();
                }
            }

            return reg;
        }

        public static List<Attendees_ClassHistory> GetAttendeesClassHistory(string email)
        {
            List<Attendees_ClassHistory> history = new List<Attendees_ClassHistory>();

            if (!String.IsNullOrEmpty(email))
            {
                using (var db = new americantraincoEntities())
                {
                    history = db.Attendees_ClassHistory.Where(h => h.AuthEmail == email || h.Email == email).ToList();
                }
            }

            return history;
        }

        public static List<REGISTRATION> GetRegistrationsByEmail(string email)
        {
            List<REGISTRATION> registrations = new List<REGISTRATION>();

            if (!String.IsNullOrEmpty(email))
            {
                using (var db = new americantraincoEntities())
                {
                    registrations = db.REGISTRATIONS.Where(p => p.RegAuthEmail == email).ToList();
                }
            }

            return registrations;
        }

        public static List<RegistrationAttendee> GetRegistrationAttendeesByEmail(string email)
        {
            List<RegistrationAttendee> attendeeList = new List<RegistrationAttendee>();

            if (!String.IsNullOrEmpty(email))
            {
                using (var db = new americantraincoEntities())
                {
                    attendeeList = db.RegistrationAttendees.Where(p => p.RegAttendeeEmail == email).ToList();
                }
            }

            return attendeeList;
        }


        public static List<RegistrationAttendee> GetRegistrationAttendees(int regId)
        {
            List<RegistrationAttendee> attendeeList = null;

            if (regId > 0)
            {
                using (var db = new americantraincoEntities())
                {
                    attendeeList = db.RegistrationAttendees.Where(p => p.RegistrationID == regId).ToList();
                }
            }

            return attendeeList;
        }

        public static List<RegistrationAttendee> GetRegistrationAttendees(IEnumerable<int> regIds)
        {
            List<RegistrationAttendee> attendeeList = null;

            if (regIds != null && regIds.Count() > 0)
            {
                using (var db = new americantraincoEntities())
                {
                    attendeeList = db.RegistrationAttendees.Where(p => regIds.Contains(p.RegistrationID)).ToList();
                }
            }

            return attendeeList;
        }

        public static List<RegistrationAttendeeSchedule> GetRegistrationAttendeesSchedules(int regId)
        {
            List<RegistrationAttendeeSchedule> attendeesScheduleList = null;

            List<RegistrationAttendee> attendeeList = GetRegistrationAttendees(regId);

            if (attendeeList != null && attendeeList.Count > 0)
            {
                using (var db = new americantraincoEntities())
                {
                    List<int> attendeeIdArray = attendeeList.Select(p => p.RegistrationAttendeeID).ToList();

                    attendeesScheduleList = db.RegistrationAttendeeSchedules.Where(p => attendeeIdArray.Contains(p.RegistrationAttendeeID)).ToList();
                }
            }

            return attendeesScheduleList;
        }

        public static List<RegistrationAttendeeSchedule> GetRegistrationAttendeesSchedulesByRegistrationAttendeeIds(IEnumerable<int> registrationAttendeeIds)
        {
            List<RegistrationAttendeeSchedule> attendeesScheduleList = new List<RegistrationAttendeeSchedule>();

            if (registrationAttendeeIds != null && registrationAttendeeIds.Count() > 0)
            {
                using (var db = new americantraincoEntities())
                {
                    attendeesScheduleList = db.RegistrationAttendeeSchedules.Where(r => registrationAttendeeIds.Contains(r.RegistrationAttendeeID)).ToList();
                }
            }

            return attendeesScheduleList;
        }


        public static List<RegistrationTrackItem> GetRegistrationTrackItems(int regId)
        {
            List<RegistrationTrackItem> trackItemListFilter = null;
            List<RegistrationTrackItem> trackItemList = new List<RegistrationTrackItem>();

            List<RegistrationAttendeeSchedule> attendeesScheduleList = GetRegistrationAttendeesSchedules(regId);

            if (attendeesScheduleList != null && attendeesScheduleList.Count > 0)
            {
                foreach (RegistrationAttendeeSchedule attendeeSchedule in attendeesScheduleList)
                {
                    int scheduleId = 0;

                    scheduleId = attendeeSchedule.ScheduleID;

                    if (scheduleId > 0)
                    {
                        SCHEDULE schedule = CacheObjects.GetScheduleList().Where(p => p.ScheduleID == scheduleId).FirstOrDefault();
                        ScheduleCourseInstructor scheduleCourse = CacheObjects.GetScheduleCourseList().Where(p => p.ScheduleID == scheduleId).FirstOrDefault();

                        if (schedule != null && scheduleCourse != null)
                        {
                            COURS course = CacheObjects.GetCourseList().Where(p => p.CourseID == scheduleCourse.CourseID).FirstOrDefault();

                            if (course != null)
                            {
                                CourseTopic topic = CacheObjects.GetCourseTopicList().Where(p => p.CourseTopicID == course.CourseTopicID).FirstOrDefault();

                                if (topic != null)
                                {
                                    RegistrationTrackItem trackItem = new RegistrationTrackItem();

                                    trackItem.Category = topic.CourseTopicName;
                                    trackItem.Code = schedule.ScheduleSeminarNumber;
                                    trackItem.Name = course.TitlePlain;
                                    trackItem.Price = Convert.ToDouble(scheduleCourse.CourseFee ?? 0);
                                    trackItem.Quantity = 1;
                                    trackItem.RegistrationId = regId;

                                    trackItemList.Add(trackItem);
                                }
                            }
                        }
                    }
                }
            }

            if (trackItemList != null && trackItemList.Count > 0)
            {
                trackItemListFilter = trackItemList.DistinctBy(p => p.Code).ToList();

                foreach (RegistrationTrackItem trackItem in trackItemListFilter)
                {
                    int itemCount = trackItemList.Where(p => p.Code == trackItem.Code).Count();

                    trackItem.Quantity = itemCount;
                }
            }


            return trackItemListFilter;
        }

        public static void EmailOrderConfirmations(CheckoutDetails checkout, REGISTRATION reg)
        {
            try
            {
                IEnumerable<IPublishedContent> emailTemplates = null;

                emailTemplates = Helpers.Nodes.SiteSettingsDirect().Children.FirstOrDefault(n => n.DocumentTypeAlias == "EmailTemplates").Children;

                if (emailTemplates != null)
                {
                    IPublishedContent emailTemplateOrderConfirm = emailTemplates.Where(p => p.Name == "Order Confirm").FirstOrDefault();
                    IPublishedContent emailTemplateOrderConfirmATI = emailTemplates.Where(p => p.Name == "Order Confirm ATI").FirstOrDefault();
                    IPublishedContent emailTemplateOrderConfirmDetail = emailTemplates.Where(p => p.Name == "Order Seminar Details Template").FirstOrDefault();
                    IPublishedContent emailTemplateOrderSummaryTemplate = emailTemplates.Where(p => p.Name == "Order Summary Template").FirstOrDefault();

                    if (emailTemplateOrderConfirm != null && emailTemplateOrderConfirmATI != null && emailTemplateOrderConfirmDetail != null)
                    {
                        Helpers.Email email = new Helpers.Email();

                        email.IsBodyHtml = true;

                        string fromOrderConfirm = emailTemplateOrderConfirm.GetProperty("emailFrom").Value.ToString();
                        string emailOrderConfirm = emailTemplateOrderConfirm.GetProperty("emailBody").Value.ToString();
                        string subjectOrderConfirm = emailTemplateOrderConfirm.GetProperty("emailSubject").Value.ToString();
                        string toAltOrderConfirm = emailTemplateOrderConfirm.GetProperty("emailToAlt").Value.ToString();

                        string fromOrderConfirmATI = emailTemplateOrderConfirmATI.GetProperty("emailFrom").Value.ToString();
                        string emailOrderConfirmATI = emailTemplateOrderConfirmATI.GetProperty("emailBody").Value.ToString();
                        string subjectOrderConfirmATI = emailTemplateOrderConfirmATI.GetProperty("emailSubject").Value.ToString();
                        string toAltOrderConfirmATI = emailTemplateOrderConfirm.GetProperty("emailToAlt").Value.ToString();

                        string emailDetailTemplate = emailTemplateOrderConfirmDetail.GetProperty("emailBody").Value.ToString();
                        string emailOrderSummaryTemplate = emailTemplateOrderSummaryTemplate.GetProperty("emailBody").Value.ToString();

                        // replace the email tags
                        emailOrderConfirm = ReplaceEmailTags(emailOrderConfirm, checkout, reg);
                        emailOrderConfirmATI = ReplaceEmailTags(emailOrderConfirmATI, checkout, reg);

                        string emailOrderAttendeeDetailsList = "";
                        string emailOrderSummaryList = "";
                        bool isCourseCancelling = false;

                        // Loop through attendees
                        foreach (temp_Reg tempReg in checkout.tempRegList)
                        {
                            string attendeeDetails = emailDetailTemplate;
                            string attendeeSummary = emailOrderSummaryTemplate;

                            SCHEDULE schedule = CacheObjects.GetScheduleList().Where(p => p.ScheduleID == tempReg.sem_SID).FirstOrDefault();
                            bool bSimulcast = schedule.ScheduleType.ToLower() == "simulcast";
                            if (schedule != null && schedule.ScheduleStatus != null && schedule.ScheduleStatus > 0)
                            {
                                isCourseCancelling = true;
                            }
                            string[] sParts = tempReg.sem_Place.Split(new string[] { " - " }, StringSplitOptions.None);
                            string seminarTitle = tempReg.sem_SID.ToString() + ": <strong>" + tempReg.sem_Title + "</strong><br />" + (bSimulcast ? "Simulcast" + (sParts.Length == 2 ? " - " + sParts[1] : "") : tempReg.sem_Place) + " - " + tempReg.sem_FeeName;
                            emailOrderSummaryList += "<tr><td colspan=\"3\">" + seminarTitle + "</td></tr><tr><td colspan=\"3\" height=\"15\"></td></tr>";


                            List<temp_Att> tempAttList = checkout.tempAttList.Where(p => p.reg_SEQ == tempReg.reg_SEQ).ToList();

                            if (tempAttList != null && tempAttList.Count > 0)
                            {
                                foreach (temp_Att tempAtt in tempAttList)
                                {
                                    if (tempAtt != null)
                                    {
                                        attendeeDetails = emailDetailTemplate;
                                        attendeeSummary = emailOrderSummaryTemplate;

                                        attendeeDetails = attendeeDetails.Replace("{{ATTENDEE}}", tempAtt.att_FName + " " + tempAtt.att_LName);
                                        attendeeDetails = attendeeDetails.Replace("{{FEE}}", string.Format("{0:C0}", tempReg.sem_FeeAmt));
                                        attendeeDetails = GenerateSeminarDetails(attendeeDetails, tempReg);

                                        emailOrderAttendeeDetailsList += attendeeDetails;


                                        attendeeSummary = attendeeSummary.Replace("{{FULL_NAME}}", tempAtt.att_FName + " " + tempAtt.att_LName);
                                        attendeeSummary = attendeeSummary.Replace("{{PRICE}}", string.Format("{0:C0}", tempReg.sem_FeeAmt));

                                        emailOrderSummaryList += attendeeSummary;
                                    }
                                }
                            }
                        }

                        emailOrderConfirm = emailOrderConfirm.Replace("{{DETAILINFO}}", emailOrderAttendeeDetailsList);
                        emailOrderConfirmATI = emailOrderConfirmATI.Replace("{{DETAILINFO}}", emailOrderAttendeeDetailsList);

                        emailOrderConfirm = emailOrderConfirm.Replace("{{ORDERSUMMARY}}", emailOrderSummaryList);
                        emailOrderConfirmATI = emailOrderConfirmATI.Replace("{{ORDERSUMMARY}}", emailOrderSummaryList);

                        if (true == isCourseCancelling)
                        {
                            string courseCancelling = "<div style=\"font-weight: bold; color: #800000; font-style: italic; text-align: center;\">";
                            courseCancelling += "** CLASS IS PENDING CANCELLATION **</div>";

                            emailOrderConfirmATI = emailOrderConfirmATI.Replace("{{XXMSG}}", courseCancelling);
                        }
                        else
                        {
                            emailOrderConfirmATI = emailOrderConfirmATI.Replace("{{XXMSG}}", "");
                        }

                        // Send email to Regsitrar
                        email.EmailFrom = fromOrderConfirm;
                        email.EmailToList = reg.RegAuthEmail.Split(';').ToList();
                        email.Subject = subjectOrderConfirm;
                        email.Body = emailOrderConfirm;

                        email.SendEmail();

                        // Send to billing email if different
                        if (reg.RegAuthEmail.Trim().ToLower() != reg.RegBillEmail.Trim().ToLower())
                        {
                            email.EmailToList = reg.RegBillEmail.Split(';').ToList();

                            email.SendEmail();
                        }

                        // Send to Admins
                        if (false == string.IsNullOrWhiteSpace(toAltOrderConfirmATI))
                        {
                            email.EmailFrom = fromOrderConfirmATI;
                            email.EmailToList = toAltOrderConfirmATI.Split(';').ToList();
                            email.Subject = (isCourseCancelling ? "** PENDING CANCELLATION **  " : "") + subjectOrderConfirmATI;
                            email.Body = emailOrderConfirmATI;

                            email.SendEmail();
                        }

                        if (true == isCourseCancelling && ConfigurationManager.AppSettings["Registration:CancelPendingEmail"] != null &&
                                ConfigurationManager.AppSettings.Get("Registration:CancelPendingEmail").Length > 0)
                        {
                            email.EmailFrom = fromOrderConfirmATI;
                            email.EmailToList = ConfigurationManager.AppSettings.Get("Registration:CancelPendingEmail").Split(';').ToList();
                            email.Subject = (isCourseCancelling ? "** PENDING CANCELLATION **  " : "") + subjectOrderConfirmATI;
                            email.Body = emailOrderConfirmATI;

                            email.SendEmail();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Registrations>("ERROR on Email Send (EmailOrderConfirmations): ", ex);
            }
        }


        public static void EmailAttendeeConfirmations(CheckoutDetails checkout, REGISTRATION reg)
        {
            try
            {
                StringBuilder authText = new StringBuilder();

                if (checkout != null && reg != null)
                {
                    IEnumerable<IPublishedContent> emailTemplates = null;

                    emailTemplates = Helpers.Nodes.Instance.SiteSettings.Children.FirstOrDefault(n => n.DocumentTypeAlias == "EmailTemplates").Children;

                    if (emailTemplates != null)
                    {
                        IPublishedContent emailTemplateAttendeeConfirm = emailTemplates.Where(p => p.Name == "Order Attendee Confirm").FirstOrDefault();
                        IPublishedContent emailTemplateOrderConfirmDetail = emailTemplates.Where(p => p.Name == "Order Attendee Details").FirstOrDefault();

                        string emailAttendeeConfirm = emailTemplateAttendeeConfirm.GetProperty("emailBody").Value.ToString();
                        string fromtAttendeeConfirm = emailTemplateAttendeeConfirm.GetProperty("emailFrom").Value.ToString();
                        string toAltAttendeeConfirm = emailTemplateAttendeeConfirm.GetProperty("emailToAlt").Value.ToString();
                        string subjectAttendeeConfirm = emailTemplateAttendeeConfirm.GetProperty("emailSubject").Value.ToString();

                        string emailDetailTemplate = emailTemplateOrderConfirmDetail.GetProperty("emailBody").Value.ToString();

                        foreach (temp_Att tempAtt in checkout.tempAttList)
                        {
                            temp_Reg tempReg = checkout.tempRegList.Where(p => p.reg_SEQ == tempAtt.reg_SEQ).FirstOrDefault();
                            string attEmailBody = emailAttendeeConfirm;

                            attEmailBody = attEmailBody.Replace("{{NAME}}", tempAtt.att_FName + " " + tempAtt.att_LName);
                            attEmailBody = attEmailBody.Replace("{{TITLE}}", tempAtt.att_Title);
                            attEmailBody = attEmailBody.Replace("{{COMPANY}}", reg.RegCompanyName);
                            attEmailBody = attEmailBody.Replace("{{DATE}}", DateTime.Now.ToShortDateString());
                            attEmailBody = attEmailBody.Replace("{{TRANSNO}}", reg.RegistrationID.ToString());
                            attEmailBody = attEmailBody.Replace("{{DETAILINFO}}", GenerateSeminarDetails(emailDetailTemplate, tempReg));

                            // Send Email to Attendee
                            Helpers.Email email = new Helpers.Email();

                            if (false == string.IsNullOrEmpty(tempAtt.att_Email))
                            {
                                email.EmailFrom = fromtAttendeeConfirm;
                                email.Subject = subjectAttendeeConfirm;
                                email.Body = attEmailBody;
                                email.IsBodyHtml = true;
                                email.EmailToList = tempAtt.att_Email.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                                email.SendEmail();
                            }

                            // Send email to Admins
                            if (false == string.IsNullOrWhiteSpace(toAltAttendeeConfirm))
                            {
                                Helpers.Email emailAdmin = new Helpers.Email();

                                emailAdmin.EmailFrom = fromtAttendeeConfirm;
                                emailAdmin.Subject = subjectAttendeeConfirm;
                                emailAdmin.Body = attEmailBody;
                                emailAdmin.IsBodyHtml = true;
                                emailAdmin.EmailToList = toAltAttendeeConfirm.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                                emailAdmin.SendEmail();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Registrations>("ERROR on Email Send (EmailAttendeeConfirmations): ", ex);
            }
        }


        private static string ReplaceEmailTags(string emailBody, CheckoutDetails checkout, REGISTRATION reg)
        {
            string output = emailBody;

            output = output.Replace("{{DOMAIN}}", HttpContext.Current.Request.Url.Authority);
            output = output.Replace("{{DATE}}", DateTime.Now.ToString("MMM dd, yyyy"));
            output = output.Replace("{{ORDERNO}}", reg.RegistrationID.ToString());
            output = output.Replace("{{ORDERNO}}", reg.RegistrationID.ToString());

            string authText = GenerateAuthDetails(checkout, reg);
            output = output.Replace("{{AUTHINFO}}", authText);

            string billingText = GenerateBillingDetails(checkout, reg);
            output = output.Replace("{{BILLINFO}}", billingText);

            string paymentInfo = GeneratePaymentDetails(checkout, reg);
            output = output.Replace("{{PAYMENTINFO}}", paymentInfo);

            output = output.Replace("{{ORDER_TOTAL}}", String.Format("{0:C0}", reg.RegOrderTotal ?? 0));

            return output;
        }


        private static string GenerateAuthDetails(CheckoutDetails checkout, REGISTRATION reg)
        {
            StringBuilder authText = new StringBuilder();

            if (checkout != null && reg != null)
            {
                authText.AppendLine(reg.RegAuthFirstName + " " + reg.RegAuthLastName + "<br />");
                if (false == string.IsNullOrEmpty(reg.RegAuthTitle))
                {
                    authText.AppendLine(reg.RegAuthTitle + "<br />");
                }
                if (false == string.IsNullOrEmpty(reg.RegCompanyName))
                {
                    authText.AppendLine(reg.RegCompanyName + "<br />");
                }
                if (false == string.IsNullOrEmpty(reg.RegAuthMailCode))
                {
                    authText.AppendLine("Mail Code: " + reg.RegAuthMailCode + "<br />");
                }
                authText.AppendLine(reg.RegAuthAddress1 + "<br />");
                if (false == string.IsNullOrEmpty(reg.RegAuthAddress2))
                {
                    authText.AppendLine(reg.RegAuthAddress2 + "<br />");
                }
                authText.AppendLine(reg.RegAuthCity + ", " + checkout.tempCust.authState + " " + reg.RegAuthZipcode + "<br />");
                authText.AppendLine(checkout.tempCust.authCountry + "<br />");

                authText.AppendLine("Phone: " + reg.RegAuthPhone);
                if (false == string.IsNullOrEmpty(reg.RegAuthPhoneExt))
                {
                    authText.AppendLine(" Ext: " + reg.RegAuthPhoneExt);
                }
                authText.AppendLine("<br />");

                if (false == string.IsNullOrEmpty(reg.RegAuthFax))
                {
                    authText.AppendLine("Fax: " + reg.RegAuthFax + "<br />");
                }
                authText.AppendLine("Email: " + reg.RegAuthEmail);
            }

            return authText.ToString();
        }


        private static string GenerateBillingDetails(CheckoutDetails checkout, REGISTRATION reg)
        {
            StringBuilder billText = new StringBuilder();

            if (checkout != null && reg != null)
            {
                billText.AppendLine(reg.RegBillFirstName + " " + reg.RegBillLastName + "<br />");
                if (false == string.IsNullOrEmpty(reg.RegBillTitle))
                {
                    billText.AppendLine(reg.RegBillTitle + "<br />");
                }
                if (false == string.IsNullOrEmpty(reg.RegCompanyName))
                {
                    billText.AppendLine(reg.RegCompanyName + "<br />");
                }
                if (false == string.IsNullOrEmpty(reg.RegBillMailCode))
                {
                    billText.AppendLine("Mail Code: " + reg.RegBillMailCode + "<br />");
                }
                billText.AppendLine(reg.RegBillAddress1 + "<br />");
                if (false == string.IsNullOrEmpty(reg.RegBillAddress2))
                {
                    billText.AppendLine(reg.RegBillAddress2 + "<br />");
                }
                billText.AppendLine(reg.RegBillCity + ", " + checkout.tempCust.authState + " " + reg.RegBillZipcode + "<br />");
                billText.AppendLine(checkout.tempCust.authCountry + "<br />");

                billText.AppendLine("Phone: " + reg.RegBillPhone);
                if (false == string.IsNullOrEmpty(reg.RegBillPhoneExt))
                {
                    billText.AppendLine(" Ext: " + reg.RegBillPhoneExt);
                }
                billText.AppendLine("<br />");

                if (false == string.IsNullOrEmpty(reg.RegBillFax))
                {
                    billText.AppendLine("Fax: " + reg.RegBillFax + "<br />");
                }
                billText.AppendLine("Email: " + reg.RegBillEmail);
            }

            return billText.ToString();
        }


        private static string GeneratePaymentDetails(CheckoutDetails checkout, REGISTRATION reg)
        {
            StringBuilder paymentText = new StringBuilder();

            if (checkout != null && reg != null)
            {
                paymentText.AppendLine("Payment Method: " + checkout.tempCust.payMethod + "<br />");

                if (checkout.tempCust.payMethod == "Credit Card")
                {
                    paymentText.AppendLine("CC Type: " + checkout.tempCust.ccType + "<br />");
                    paymentText.AppendLine("CC Number: ****-****-****-" + StringUtilities.GetLast(checkout.tempCust.ccNumber, 4) + "<br />");
                    paymentText.AppendLine("CC Expire: **/**<br />");
                }
                else
                {
                    if (false == string.IsNullOrEmpty(reg.RegBillPONumber))
                    {
                        paymentText.AppendLine("PO Number: " + reg.RegBillPONumber + "<br />");
                    }
                }
            }

            return paymentText.ToString();
        }



        private static string GenerateOrderDetails(CheckoutDetails checkout, REGISTRATION reg)
        {
            StringBuilder detailsText = new StringBuilder();

            if (checkout != null && reg != null)
            {
                foreach (temp_Reg tempReg in checkout.tempRegList)
                {

                }
            }


            return detailsText.ToString();
        }


        private static string GenerateSeminarDetailsList(string emailTemplate, CheckoutDetails checkout)
        {
            StringBuilder detailsText = new StringBuilder();

            if (checkout != null && checkout.tempCust != null)
            {
                foreach (temp_Reg tempReg in checkout.tempRegList)
                {
                    detailsText.AppendLine(GenerateSeminarDetails(emailTemplate, tempReg));
                }
            }


            return detailsText.ToString();
        }


        private static string GenerateSeminarDetails(string emailTemplate, temp_Reg tempReg)
        {
            StringBuilder detailsText = new StringBuilder();
            Carts cartsObj = new Carts();

            if (true == string.IsNullOrEmpty(emailTemplate))
            {
                cartsObj.SendCheckoutErrorEmail("GenerateSeminarDetails: emailTemplate = NULL or Empty");

                return null;
            }

            if (tempReg != null)
            {
                SCHEDULE schedule = Objects.CacheObjects.GetScheduleList().Where(p => p.ScheduleID == tempReg.sem_SID).First();
                bool bSimulcast = schedule.ScheduleType.ToLower() == "simulcast";
                ScheduleCourseInstructor schCourse = Objects.CacheObjects.GetScheduleCourseList().Where(p => p.ScheduleID == tempReg.sem_SID).FirstOrDefault();
                Location location = null;
                if(!bSimulcast)
                    location = Objects.CacheObjects.GetLocationList().Where(p => p.LocationID == schedule.LocationID).FirstOrDefault();
                COURS course = Objects.CacheObjects.GetCourseList().Where(p => p.CourseID == schCourse.CourseID).FirstOrDefault();
                string tempStr = emailTemplate;
                IPublishedContent searchSeminarNode = null;
                searchSeminarNode = Nodes.Instance.SeminarSearch;
                string[] sParts = tempReg.sem_Place.Split(new string[] { " - " }, StringSplitOptions.None);
                tempStr = tempStr.Replace("{{SEMINAR}}", tempReg.sem_SID + ": " + tempReg.sem_Title + " - " + (bSimulcast ? "Simulcast" + (sParts.Length == 2 ? " - " + sParts[1] : "") : tempReg.sem_Place) + " - " + tempReg.sem_FeeName);
                string timeStr = "";
                if(!bSimulcast && course != null)
                    timeStr = course.CourseTimes;
                else if(searchSeminarNode.GetProperty("simulcastInfoMessage").HasValue)
                    timeStr = searchSeminarNode.GetProperty("simulcastInfoMessage").Value.ToString();
                tempStr = tempStr.Replace("{{TIME}}", timeStr);
                string locationStr = " - ";
                if (location != null && !string.IsNullOrEmpty(location.LocationNotes))
                    locationStr = FixLocationNotes(location.LocationNotes);
                else if (!bSimulcast && searchSeminarNode.GetProperty("locationMessage").HasValue)
                    locationStr = searchSeminarNode.GetProperty("locationMessage").Value.ToString();
                tempStr = tempStr.Replace("{{LOCATION}}",locationStr);
                detailsText.AppendLine(tempStr);
            }
            else
            {
                cartsObj.SendCheckoutErrorEmail("GenerateSeminarDetails: tempReg = NULL");

                return null;
            }


            return detailsText.ToString();
        }


        private static string FixLocationNotes(string locationNotes)
        {
            string output = "";

            if (false == string.IsNullOrEmpty(locationNotes))
            {
                output = locationNotes;

                output = output.Replace("</ul>", "</ul><br />" + Environment.NewLine);
                output = output.Replace(Environment.NewLine, "<br />" + Environment.NewLine);
                output = output.Replace("\r\n", "<br />" + Environment.NewLine);
                output = output.Replace("\"", "&quot;");
            }

            return output;
        }

        public static void EmailPaymentConfirmation(CheckoutCustomer model)
        {
            try
            {
                StringBuilder emailText = new StringBuilder();
                if (model == null || string.IsNullOrEmpty(model.Email))
                    return;
                IPublishedContent template = Helpers.Nodes.Instance.SiteSettings.Children.FirstOrDefault(n => n.DocumentTypeAlias == "EmailTemplates").Children.Where(p => p.Name == "Payment Confirmation").FirstOrDefault();
                if (template == null)
                    return;
                string emailBody = template.GetProperty("emailBody").Value.ToString();
                string toAlt = template.GetProperty("emailToAlt").Value.ToString();
                emailBody = emailBody.Replace("{{COMPANY}}", model.Company)
                                     .Replace("{{NAME}}", model.FirstName + " " + model.LastName)
                                     .Replace("{{INVOICE}}", model.InvoiceNumber)
                                     .Replace("{{DATE}}", DateTime.Now.ToShortDateString())
                                     .Replace("{{AMOUNT}}", model.Amount.Value.ToString("C", new CultureInfo("en-US")))
                                     .Replace("{{CARDTYPE}}", StringUtilities.CreditCardType(model.CCNumber))
                                     .Replace("{{CARDNUM}}", "***" + model.CCNumber.Substring(model.CCNumber.Length - 4, 4));
                Helpers.Email email = new Helpers.Email();
                email.EmailFrom = template.GetProperty("emailFrom").Value.ToString();
                email.Subject = template.GetProperty("emailSubject").Value.ToString();
                email.Body = emailBody;
                email.IsBodyHtml = true;
                List<string> emails = new List<string> { model.Email };
                if(!string.IsNullOrEmpty(toAlt))
                    emails.AddRange(toAlt.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList());
                email.EmailToList = emails;
                email.SendEmail();
            }
            catch (Exception ex)
            {
                LogHelper.Error<Registrations>("ERROR on Email Send (PaymentConfirmation): ", ex);
            }
        }
    }
}

