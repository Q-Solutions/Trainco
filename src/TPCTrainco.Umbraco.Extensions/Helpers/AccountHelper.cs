using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TPCTrainco.Umbraco.Extensions.Models.Account;
using TPCTrainco.Umbraco.Extensions.Objects;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using System.Data.Entity.Validation;

namespace TPCTrainco.Umbraco.Extensions.Helpers
{
    public static class AccountHelper
    {
        private const string _alg = "HmacSHA256";
        private const string _salt = "roduMbjVKvk5UNTuvkXUf"; // Generated at https://www.random.org/strings
        private const int _expirationMinutes = 360;
        private const int _validationExpirationMinutes = 360;

        public static string GenerateToken(string memberKey, string email, string host, string ip, string userAgent, long ticks)
        {
            var member = ApplicationContext.Current.Services.MemberService.GetByKey(new Guid(memberKey));

            var rawPassword = member.RawPasswordValue;

            string hash = string.Join(":", new string[] { rawPassword, memberKey.ToString(), email, ip, userAgent, ticks.ToString() });
            string hashLeft = "";
            string hashRight = "";

            using (HMAC hmac = HMACSHA256.Create(_alg))
            {
                hmac.Key = Encoding.UTF8.GetBytes(GetHashedPassword(host));
                hmac.ComputeHash(Encoding.UTF8.GetBytes(hash));

                hashLeft = Convert.ToBase64String(hmac.Hash);
                hashRight = string.Join(":", new string[] { memberKey.ToString(), email, ticks.ToString() });
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(":", hashLeft, hashRight)));
        }

        public static string GetHashedPassword(string password)
        {
            string key = string.Join(":", new string[] { password, _salt });

            using (HMAC hmac = HMACSHA256.Create(_alg))
            {
                // Hash the key.
                hmac.Key = Encoding.UTF8.GetBytes(_salt);
                hmac.ComputeHash(Encoding.UTF8.GetBytes(key));

                return Convert.ToBase64String(hmac.Hash);
            }
        }

        public static bool IsTokenValid(string token, string host, string ip, string userAgent)
        {
            bool result = false;

            try
            {
                // Base64 decode the string, obtaining the token:username:timeStamp.
                string key = Encoding.UTF8.GetString(Convert.FromBase64String(token));

                // Split the parts.
                string[] parts = key.Split(new char[] { ':' });
                if (parts.Length == 4)
                {
                    // Get the hash message, username, and timestamp.
                    string hash = parts[0];
                    string memberKey = parts[1];
                    string email = parts[2];
                    long ticks = long.Parse(parts[3]);
                    DateTime timeStamp = new DateTime(ticks);

                    bool expired = (DateTime.UtcNow - timeStamp).TotalMinutes > _expirationMinutes;
                    if (!expired)
                    {
                        // Hash the message with the key to generate a token.
                        string computedToken = GenerateToken(memberKey, email, host, ip, userAgent, ticks);

                        // Compare the computed token with the one supplied and ensure they match.
                        result = (token == computedToken);
                    }
                }
            }
            catch
            {
            }

            return result;
        }

        public static string GetEmailFromToken(string token)
        {
            var email = String.Empty;
            try
            {
                // Base64 decode the string, obtaining the token:username:timeStamp.
                string key = Encoding.UTF8.GetString(Convert.FromBase64String(token));

                // Split the parts.
                string[] parts = key.Split(new char[] { ':' });
                if (parts.Length == 4)
                {
                    email = parts[2];
                }
            }
            catch
            {
            }

            return email;
        }

        public static string GetMemberKeyFromToken(string token)
        {
            var memberKey = String.Empty;
            try
            {
                // Base64 decode the string, obtaining the token:username:timeStamp.
                string key = Encoding.UTF8.GetString(Convert.FromBase64String(token));

                // Split the parts.
                string[] parts = key.Split(new char[] { ':' });
                if (parts.Length == 4)
                {
                    memberKey = parts[1];
                }
            }
            catch
            {
            }

            return memberKey;
        }

        public static UserModel CreateUser(UserModel user, Uri uri)
        {
            try
            {
                // Check if user has been verified
                if (ApplicationContext.Current.Services.MemberService.Exists(user.Email))
                {
                    LogHelper.Info(typeof(AccountHelper), "Create User: Email Found: " + user.Email);

                    var member = ApplicationContext.Current.Services.MemberService.GetByEmail(user.Email);

                    if (false == member.IsApproved)
                    {
                        LogHelper.Info(typeof(AccountHelper), "Create User: Member not approved. Deleting: " + user.Email);

                        ApplicationContext.Current.Services.MemberService.Delete(member);
                    }
                }


                if (!ApplicationContext.Current.Services.MemberService.Exists(user.Email))
                {
                    var member = ApplicationContext.Current.Services.MemberService.CreateMember(
                        username: user.Email,
                        email: user.Email,
                        name: String.Format("{0} {1}", user.FirstName, user.LastName),
                        memberTypeAlias: "Member"
                    );

                    member.SetValue("firstName", user.FirstName);
                    member.SetValue("lastName", user.LastName);
                    member.SetValue("title", user.Title);
                    member.SetValue("phone", user.Phone);
                    member.SetValue("phoneExtension", user.PhoneExtension);
                    member.SetValue("favoritedCourses", user.FavoritedCourses);

                    var validationCode = Guid.NewGuid().ToString();
                    member.SetValue("validationCode", String.Join(":", validationCode, DateTime.UtcNow.Ticks));

                    member.SetValue("umbracoMemberApproved", false);

                    member.Name = String.Format("{0} {1}", user.FirstName, user.LastName);

                    //member.RawPasswordValue = user.Password;                   

                    ApplicationContext.Current.Services.MemberService.Save(member);

                    if (String.IsNullOrWhiteSpace(user.Password))
                    {
                        user.Password = validationCode;
                    }

                    ApplicationContext.Current.Services.MemberService.SavePassword(member, user.Password);

                    user.Key = member.Key.ToString();
                    user.ValidationCode = validationCode;
                    UpdateUserTrainco(user);
                    var emailTemplete = GetEmailTemplate("New Account Verification");

                    if (emailTemplete != null)
                    {
                        string from = emailTemplete.GetProperty("emailFrom").Value.ToString();
                        string body = emailTemplete.GetProperty("emailBody").Value.ToString();
                        string subject = emailTemplete.GetProperty("emailSubject").Value.ToString();
                        string bcc = emailTemplete.GetProperty("emailToAlt").Value.ToString();
                        string siteUrlBase = String.Format("{0}://{1}", uri.Scheme, uri.Host);

                        var smtp = new Email();
                        smtp.EmailFrom = from;
                        smtp.EmailToList = new List<string>() { user.Email };
                        smtp.EmailBccList = bcc.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        smtp.IsBodyHtml = true;
                        smtp.Subject = subject;
                        smtp.Body = body.Replace("{{FIRSTNAME}}", user.FirstName).Replace("{{LASTNAME}}", user.LastName).Replace("{{VALIDATIONCODE}}", validationCode).Replace("{{SITEBASEURL}}", siteUrlBase).Replace("{{EMAIL}}", user.Email);
                        smtp.SendEmail();

                        if (false == string.IsNullOrEmpty(user.Email))
                        {
                            LogHelper.Info(typeof(AccountHelper), "Create User: New Account Verification Email Sent: " + user.Email);
                        }
                        else
                        {
                            LogHelper.Error(typeof(AccountHelper), "Create User: New Account Verification: To Email is empty", null);
                        }
                    }
                    else
                    {
                        LogHelper.Error(typeof(AccountHelper), "Create User: Could not find Email Template: New Account Verification", null);
                    }
                }
                else
                {
                    LogHelper.Info(typeof(AccountHelper), "Create User: Member found and already verified: " + user.Email);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(AccountHelper), "Create User: Error creating new trainco member account.", ex);
            }

            return user;
        }

        public static bool CheckUserAccount(string email)
        {
            bool exists = false;

            exists = ApplicationContext.Current.Services.MemberService.Exists(email);

            return exists;
        }

        public static bool ForgottenPassword(string email, Uri uri)
        {
            var success = true;

            var member = ApplicationContext.Current.Services.MemberService.GetByEmail(email);

            if (member != null)
            {
                try
                {
                    var validationCode = Guid.NewGuid().ToString();
                    member.SetValue("validationCode", String.Join(":", validationCode, DateTime.UtcNow.Ticks));

                    ApplicationContext.Current.Services.MemberService.Save(member);

                    var emailTemplete = GetEmailTemplate("Forgotten Password");

                    if (emailTemplete != null)
                    {
                        string from = emailTemplete.GetProperty("emailFrom").Value.ToString();
                        string body = emailTemplete.GetProperty("emailBody").Value.ToString();
                        string subject = emailTemplete.GetProperty("emailSubject").Value.ToString();
                        string bcc = emailTemplete.GetProperty("emailToAlt").Value.ToString();
                        string siteUrlBase = String.Format("{0}://{1}", uri.Scheme, uri.Host);

                        var smtp = new Email();
                        smtp.EmailFrom = from;
                        smtp.EmailToList = new List<string>() { email };
                        smtp.EmailBccList = bcc.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        smtp.IsBodyHtml = true;
                        smtp.Subject = subject;
                        smtp.Body = body.Replace("{{FIRSTNAME}}", member.GetValue<string>("firstName")).Replace("{{LASTNAME}}", member.GetValue<string>("firstName")).Replace("{{VALIDATIONCODE}}", validationCode).Replace("{{SITEBASEURL}}", siteUrlBase);
                        success = smtp.SendEmail();
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    LogHelper.Error(typeof(AccountHelper), "There was a problem sending forgotten password email.", ex);
                }
            }
            else
            {
                success = false;
            }

            return success;
        }

        public static bool UpdateSaveForLater(string memberKey, string courseId)
        {
            var success = true;

            var member = ApplicationContext.Current.Services.MemberService.GetByKey(new Guid(memberKey));

            if (member != null && !String.IsNullOrEmpty(courseId))
            {
                var favoritedCoursesRaw = member.GetValue<string>("favoritedCourses");

                var favoritedCourses = String.IsNullOrEmpty(favoritedCoursesRaw) ? new List<string>() : favoritedCoursesRaw.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()).ToList();
                if (!favoritedCourses.Contains(courseId))
                {
                    favoritedCourses.Add(courseId.Trim());
                    member.SetValue("favoritedCourses", String.Join(",", favoritedCourses));

                    try
                    {
                        ApplicationContext.Current.Services.MemberService.Save(member);
                    }
                    catch
                    {
                        success = false;
                    }
                }
            }
            else
            {
                success = false;
            }

            return success;
        }

        public static bool EmailVerification(string email, string validationCode, out IMember member)
        {
            var success = true;

            member = ApplicationContext.Current.Services.MemberService.GetByUsername(email);

            if (member != null && member.GetValue<string>("ValidationCode").StartsWith(validationCode))
            {
                LogHelper.Info(typeof(AccountHelper), "Email Verification: Member found and validation code accepted: " + email + " | Code: " + validationCode);

                try
                {
                    LogHelper.Info(typeof(AccountHelper), "Email Verification: Saving member: " + email);

                    member.IsApproved = true;
                    member.SetValue("umbracoMemberApproved", true);
                    ApplicationContext.Current.Services.MemberService.Save(member);

                    member = ApplicationContext.Current.Services.MemberService.GetByUsername(email);

                    if (true == member.IsApproved)
                    {
                        LogHelper.Info(typeof(AccountHelper), "Email Verification: Member set to approved: " + email);
                    }
                    else
                    {
                        LogHelper.Error(typeof(AccountHelper), "Email Verification: ERROR! Member NOT set to approved: " + email, null);
                    }
                    member.IsApproved = true;
                }
                catch (Exception ex)
                {
                    success = false;
                    LogHelper.Error(typeof(AccountHelper), "Email Verification: ERROR saving member: " + email, ex);
                }
            }
            else
            {
                success = false;
                LogHelper.Warn(typeof(AccountHelper), "Email Verification: Could not find user by email address: " + email);
            }

            return success;
        }

        public static void ShareCourse(string memberKey, string toEmail, int courseId, Uri uri)
        {
            var emailTemplete = GetEmailTemplate("Share Course");

            if (emailTemplete != null)
            {
                string from = emailTemplete.GetProperty("emailFrom").Value.ToString();
                string body = emailTemplete.GetProperty("emailBody").Value.ToString();
                string subject = emailTemplete.GetProperty("emailSubject").Value.ToString();
                string bcc = emailTemplete.GetProperty("emailToAlt").Value.ToString();

                Seminars seminars = new Seminars();
                var course = CacheObjects.GetCourseDetailList().FirstOrDefault(c => c.Id == courseId);

                if (course != null)
                {
                    string couresUrl = String.Format("{0}://{1}{2}", uri.Scheme, uri.Host, course.DetailsUrl);

                    var member = ApplicationContext.Current.Services.MemberService.GetByKey(new Guid(memberKey));

                    var smtp = new Email();
                    smtp.EmailFrom = member.Email; // TODO: Should this be from the system or the user?
                    smtp.EmailToList = new List<string>() { toEmail };
                    smtp.EmailBccList = bcc.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    smtp.IsBodyHtml = true;
                    smtp.Subject = subject;
                    smtp.Body = body.Replace("{{FIRSTNAME}}", member.GetValue<string>("firstName")).Replace("{{LASTNAME}}", member.GetValue<string>("firstName")).Replace("{{COURSEURL}}", couresUrl);
                    smtp.SendEmail();
                }
            }
        }

        public static UserModel GetUser(string memberKey)
        {
            UserModel user = null;

            var member = ApplicationContext.Current.Services.MemberService.GetByKey(new Guid(memberKey));

            if (member != null)
            {
                user = new UserModel()
                {
                    Key = member.Key.ToString(),
                    Email = member.Email,
                    FirstName = member.GetValue<string>("firstName"),
                    LastName = member.GetValue<string>("lastName"),
                    Title = member.GetValue<string>("title"),
                    Phone = member.GetValue<string>("phone"),
                    PhoneExtension = member.GetValue<string>("phoneExtension"),
                    FavoritedCourses = member.GetValue<string>("favoritedCourses"),
                    ValidationCode = member.GetValue<string>("validationCode")
                };
            }

            return user;
        }

        public static CompanyModel GetCompany(string memberKey)
        {
            CompanyModel company = null;

            var member = ApplicationContext.Current.Services.MemberService.GetByKey(new Guid(memberKey));

            if (member != null)
            {
                company = new CompanyModel()
                {
                    HowDidYouAboutUs = member.GetValue<string>("howDidYouAboutUs"),
                    PromCode = member.GetValue<string>("promoCode"),
                    Name = member.GetValue<string>("companyName"),
                    Address1 = member.GetValue<string>("address1"),
                    Address2 = member.GetValue<string>("address2"),
                    Country = member.GetValue<string>("country"),
                    City = member.GetValue<string>("city"),
                    State = member.GetValue<string>("state"),
                    PostalCode = member.GetValue<string>("postalCode"),
                    Industry = member.GetValue<string>("industry"),
                    Role = member.GetValue<string>("role"),
                    ExternalTrainingUsageAmount = member.GetValue<string>("extentalTrianingUsageAmount"),
                    NumberOfEmployees = member.GetValue<string>("numberOfEmployees"),
                    TrainingTopics = member.GetValue<string>("trainingTopics"),
                    HasMakePreviousPurchase = member.GetValue<string>("hasMakePreviousPurchase")
                };
            }

            return company;
        }

        public static BillingModel GetBilling(string memberKey)
        {
            BillingModel billing = null;

            var member = ApplicationContext.Current.Services.MemberService.GetByKey(new Guid(memberKey));

            if (member != null)
            {
                billing = new BillingModel()
                {
                    CompanyName = member.GetValue<string>("billingCompanyName"),
                    FirstName = member.GetValue<string>("billingFirstName"),
                    LastName = member.GetValue<string>("billingLastName"),
                    Email = member.GetValue<string>("billingEmail"),
                    Address1 = member.GetValue<string>("billingAddress1"),
                    Address2 = member.GetValue<string>("billingAddress2"),
                    Country = member.GetValue<string>("billingCountry"),
                    City = member.GetValue<string>("billingCity"),
                    State = member.GetValue<string>("billingState"),
                    PostalCode = member.GetValue<string>("billingPostalCode")
                };
            }

            return billing;
        }

        public static IEnumerable<CourseModel> GetSaveForLater(string memberKey)
        {
            var courses = new List<CourseModel>();

            var member = ApplicationContext.Current.Services.MemberService.GetByKey(new Guid(memberKey));

            if (member != null)
            {
                var favoritedCoursesRaw = member.GetValue<string>("favoritedCourses");
                var courseIds = String.IsNullOrEmpty(favoritedCoursesRaw) ? Enumerable.Empty<int>() : member.GetValue<string>("favoritedCourses").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => Convert.ToInt32(c.Trim()));

                foreach (var courseId in courseIds)
                {
                    var courseDetail = CacheObjects.GetCourseDetailList().FirstOrDefault(c => c.Id == courseId);
                    if (courseDetail != null)
                    {
                        courses.Add(new CourseModel()
                        {
                            CourseDetail = courseDetail
                        });
                    }
                }
            }

            return courses;
        }

        public static bool UpdateCompany(string memberKey, CompanyModel company)
        {
            var success = true;

            var member = ApplicationContext.Current.Services.MemberService.GetByKey(new Guid(memberKey));

            if (member != null)
            {
                member.SetValue("companyName", company.Name);
                member.SetValue("address1", company.Address1);
                member.SetValue("address2", company.Address2);
                member.SetValue("country", company.Country);
                member.SetValue("city", company.City);
                member.SetValue("state", company.State);
                member.SetValue("postalCode", company.PostalCode);

                member.SetValue("howDidYouAboutUs", company.HowDidYouAboutUs);
                member.SetValue("promoCode", company.PromCode);
                member.SetValue("industry", company.Industry);
                member.SetValue("role", company.Role);
                member.SetValue("extentalTrianingUsageAmount", company.ExternalTrainingUsageAmount);
                member.SetValue("numberOfEmployees", company.NumberOfEmployees);
                member.SetValue("trainingTopics", company.TrainingTopics);
                member.SetValue("hasMakePreviousPurchase", company.HasMakePreviousPurchase);

                try
                {
                    ApplicationContext.Current.Services.MemberService.Save(member);
                    UpdateCompanyTrainco(member.Email, company);
                }
                catch
                {
                    success = false;
                }
            }
            else
            {
                success = false;
            }

            return success;
        }

        public static bool UpdateBilling(string memberKey, BillingModel billing)
        {
            var success = true;

            var member = ApplicationContext.Current.Services.MemberService.GetByKey(new Guid(memberKey));

            if (member != null)
            {
                member.SetValue("billingCompanyName", billing.CompanyName);
                member.SetValue("billingFirstName", billing.FirstName);
                member.SetValue("billingLastName", billing.LastName);
                member.SetValue("billingEmail", billing.Email);
                member.SetValue("billingAddress1", billing.Address1);
                member.SetValue("billingAddress2", billing.Address2);
                member.SetValue("billingCountry", billing.Country);
                member.SetValue("billingCity", billing.City);
                member.SetValue("billingState", billing.State);
                member.SetValue("billingPostalCode", billing.PostalCode);

                try
                {
                    ApplicationContext.Current.Services.MemberService.Save(member);
                }
                catch
                {
                    success = false;
                }
            }
            else
            {
                success = false;
            }

            return success;
        }

        public static bool UpdateUser(string memberKey, UserModel user, out IMember member)
        {
            var success = true;

            member = ApplicationContext.Current.Services.MemberService.GetByKey(new Guid(memberKey));

            if (member != null)
            {
                member.SetValue("firstName", user.FirstName);
                member.SetValue("lastName", user.LastName);
                member.SetValue("title", user.Title);
                member.SetValue("phone", user.Phone);
                member.SetValue("phoneExtension", user.PhoneExtension);

                member.Username = user.Email;
                member.Email = user.Email;

                try
                {
                    ApplicationContext.Current.Services.MemberService.Save(member);
                    UpdateUserTrainco(user, false);
                }
                catch
                {
                    success = false;
                }
            }
            else
            {
                success = false;
            }

            return success;
        }

        public static bool DeleteSaveForLater(string memberKey, string courseId)
        {
            var success = true;

            var member = ApplicationContext.Current.Services.MemberService.GetByKey(new Guid(memberKey));

            if (member != null && !String.IsNullOrEmpty(courseId))
            {
                var favoritedCoursesRaw = member.GetValue<string>("favoritedCourses");

                var favoritedCourses = favoritedCoursesRaw.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()).ToList();
                if (favoritedCourses.Contains(courseId))
                {
                    favoritedCourses.Remove(courseId);
                    member.SetValue("favoritedCourses", String.Join(",", favoritedCourses));

                    try
                    {
                        ApplicationContext.Current.Services.MemberService.Save(member);
                    }
                    catch
                    {
                        success = false;
                    }
                }
            }
            else
            {
                success = false;
            }

            return success;
        }

        public static bool UpdatePassword(string email, string password, string validationCode, out IMember member)
        {
            var success = false;

            member = ApplicationContext.Current.Services.MemberService.GetByEmail(email);

            if (member != null && !String.IsNullOrEmpty(validationCode))
            {
                var memberValidationCode = member.GetValue<string>("validationCode");
                if (!String.IsNullOrEmpty(memberValidationCode) && memberValidationCode.Contains(":"))
                {
                    var validationCodeAndTime = memberValidationCode.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                    if (validationCodeAndTime.Length == 2)
                    {
                        var matches = validationCodeAndTime[0].Equals(validationCode);

                        long ticks = long.Parse(validationCodeAndTime[1]);
                        DateTime timeStamp = new DateTime(ticks);
                        bool expired = (DateTime.UtcNow - timeStamp).TotalMinutes > _expirationMinutes;

                        if (!expired && matches)
                        {
                            success = ResetPassword(member, password);
                        }
                        else if (expired)
                        {
                            member.SetValue("validationCode", String.Empty);
                            ApplicationContext.Current.Services.MemberService.Save(member);
                        }
                    }
                }
                else if (String.IsNullOrEmpty(memberValidationCode) && validationCode.Equals(member.Key.ToString()))
                {
                    success = ResetPassword(member, password);
                }
            }

            return success;
        }

        private static bool ResetPassword(IMember member, string password)
        {
            var success = true;
            try
            {
                ApplicationContext.Current.Services.MemberService.SavePassword(member, password);

                member.IsApproved = true;
                member.SetValue("validationCode", String.Empty);
                ApplicationContext.Current.Services.MemberService.Save(member);
            }
            catch
            {
                success = false;
            }
            return success;
        }

        public static IPublishedContent GetEmailTemplate(string name)
        {
            IPublishedContent emailTemplate = null;

            IEnumerable<IPublishedContent> emailTemplates = Helpers.Nodes.SiteSettingsDirect().Children.FirstOrDefault(n => n.DocumentTypeAlias == "EmailTemplates").Children;

            if (emailTemplates != null)
            {
                emailTemplate = emailTemplates.Where(p => p.Name == name).FirstOrDefault();
            }

            return emailTemplate;
        }

        public static string GetSupervisorEmail(string email)
        {
            var supervisorsEmail = String.Empty;

            var attendee = Registrations.GetRegistrationAttendeesByEmail(email).OrderByDescending(a => a.RegistrationAttendeeID).FirstOrDefault();

            if (attendee != null)
            {
                var registration = Registrations.GetRegistrationById(attendee.RegistrationID);

                if (registration != null)
                {
                    supervisorsEmail = registration.RegAuthEmail;
                }
            }

            return supervisorsEmail;
        }

        public static string GetSupervisorMemberKey(string email)
        {
            var supervisorsMemberKey = String.Empty;
            var supervisorsEmail = String.Empty;

            var attendee = Registrations.GetRegistrationAttendeesByEmail(email).OrderByDescending(a => a.RegistrationAttendeeID).FirstOrDefault();

            if (attendee != null)
            {
                var registration = Registrations.GetRegistrationById(attendee.RegistrationID);

                if (registration != null)
                {
                    supervisorsEmail = registration.RegAuthEmail;
                }
            }

            var member = ApplicationContext.Current.Services.MemberService.GetByEmail(supervisorsEmail);
            if (member != null)
            {
                supervisorsMemberKey = member.Key.ToString();
            }

            return supervisorsMemberKey;
        }

        public static IEnumerable<CourseModel> GetUpcomingCourses(string memberKey, string memberEmail)
        {
            var courses = GetCoursesByEmail(memberEmail);
            return courses.Where(c => c.ScheduleDate > DateTime.Now);
        }

        public static IEnumerable<CourseModel> GetPastCourses(string memberKey, string memberEmail)
        {
            var courses = GetCoursesByEmail(memberEmail);
            return courses.Where(c => c.ScheduleDate <= DateTime.Now);
        }

        private static List<CourseModel> GetCoursesByEmail(string memberEmail)
        {
            var courses = new List<CourseModel>();

            var historyRecords = Registrations.GetAttendeesClassHistory(memberEmail);

            foreach (var historyGroup in historyRecords.GroupBy(h => h.CourseID))
            {
                var courseDetail = CacheObjects.GetCourseDetailList().FirstOrDefault(c => c.Id == historyGroup.First().CourseID);
                var scheduleDetail = CacheObjects.GetScheduleListAll().FirstOrDefault(c => c.ScheduleID == historyGroup.First().ScheduleID);

                if (courseDetail != null && scheduleDetail != null)
                {
                    var content = ApplicationContext.Current.Services.ContentService.GetById(courseDetail.NodeId);

                    var materials = new List<MaterialModel>();
                    string materialIdsRaw = "";
                    if(content != null)
                        materialIdsRaw = content.GetValue<string>("materials");

                    if (!String.IsNullOrEmpty(materialIdsRaw))
                    {
                        var materialIds = materialIdsRaw.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt32(i));
                        foreach (var materialId in materialIds)
                        {
                            var media = ApplicationContext.Current.Services.MediaService.GetById(materialId);
                            if (media != null)
                            {
                                materials.Add(new MaterialModel()
                                {
                                    Url = media.GetValue<string>("umbracoFile")
                                });
                            }
                        }
                    }

                    var attendeeList = new List<AttendeeModel>();
                    foreach (var history in historyGroup)
                    {
                        attendeeList.Add(new AttendeeModel()
                        {
                            Email = history.Email,
                            FirstName = history.FirstName,
                            LastName = history.LastName,
                            Title = history.Title
                        });
                    }

                    courses.Add(new CourseModel()
                    {
                        CourseDetail = courseDetail,
                        ScheduleId = scheduleDetail.ScheduleID,
                        ScheduleDate = scheduleDetail.ScheduleDate,
                        ScheduleDateDescription = scheduleDetail.ScheduleDateDescription,
                        Materials = materials,
                        Attendees = attendeeList
                    });
                }
            }

            return courses;
        }

        private static List<CourseModel> GetCourses(string memberKey)
        {
            var courses = new List<CourseModel>();

            // get the member so we can get their email address
            var member = ApplicationContext.Current.Services.MemberService.GetByKey(new Guid(memberKey));

            if (member != null)
            {
                List<RegistrationAttendee> attendees = new List<RegistrationAttendee>();

                var registrationIds = Registrations.GetRegistrationsByEmail(member.Email).Select(r => r.RegistrationID);

                // This adds all the attendees purchased by the member.
                var registrationAttendees = Registrations.GetRegistrationAttendees(registrationIds);
                if (registrationAttendees != null)
                {
                    attendees.AddRange(Registrations.GetRegistrationAttendees(registrationIds));
                }

                // This adds all the attendee records that are the member but where purchased by someone else. It compares id's and only add records that are not already in the collection.
                attendees.AddRange(Registrations.GetRegistrationAttendeesByEmail(member.Email).Where(r => attendees.Select(a => a.RegistrationAttendeeID).Contains(r.RegistrationAttendeeID) == false));

                // Grab all the RegistrationAttendeeSchedule records to reduce trips to the database.
                var registrationAttendeeSchedules = Registrations.GetRegistrationAttendeesSchedulesByRegistrationAttendeeIds(attendees.Select(a => a.RegistrationAttendeeID));

                foreach (var attendee in attendees)
                {
                    var registrationAttendeeSchedule = registrationAttendeeSchedules.Where(r => r.RegistrationAttendeeID == attendee.RegistrationAttendeeID);

                    var scheduleId = registrationAttendeeSchedule.First().ScheduleID;

                    var course = courses.FirstOrDefault(c => c.ScheduleId == scheduleId);

                    if (course == null)
                    {
                        var scheduleCourseInstructor = CacheObjects.GetScheduleCourseList().Where(i => i.ScheduleID == scheduleId);

                        if (scheduleCourseInstructor != null && scheduleCourseInstructor.Count() > 0)
                        {
                            var courseId = scheduleCourseInstructor.First().CourseID;

                            var courseDetail = CacheObjects.GetCourseDetailList().FirstOrDefault(c => c.Id == courseId);
                            var scheduleDetail = CacheObjects.GetScheduleListAll().FirstOrDefault(c => c.ScheduleID == scheduleId);

                            if (courseDetail != null && scheduleDetail != null)
                            {
                                var content = ApplicationContext.Current.Services.ContentService.GetById(courseDetail.NodeId);

                                var materials = new List<MaterialModel>();

                                var materialIdsRaw = content.GetValue<string>("materials");

                                if (!String.IsNullOrEmpty(materialIdsRaw))
                                {
                                    var materialIds = materialIdsRaw.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt32(i));
                                    foreach (var materialId in materialIds)
                                    {
                                        var media = ApplicationContext.Current.Services.MediaService.GetById(materialId);
                                        if (media != null)
                                        {
                                            materials.Add(new MaterialModel()
                                            {
                                                Url = media.GetValue<string>("umbracoFile")
                                            });
                                        }
                                    }
                                }

                                var attendeeList = new List<AttendeeModel>();
                                attendeeList.Add(new AttendeeModel()
                                {
                                    Email = attendee.RegAttendeeEmail,
                                    FirstName = attendee.RegAttendeeFirstName,
                                    LastName = attendee.RegAttendeeLastName,
                                    Title = attendee.RegAttendeeTitle
                                });

                                courses.Add(new CourseModel()
                                {
                                    CourseDetail = courseDetail,
                                    ScheduleId = scheduleDetail.ScheduleID,
                                    ScheduleDate = scheduleDetail.ScheduleDate,
                                    ScheduleDateDescription = scheduleDetail.ScheduleDateDescription,
                                    Materials = materials,
                                    Attendees = attendeeList
                                });
                            }
                        }
                    }
                    else if (!course.Attendees.Any(a => a.RegistrationAttendeeId == attendee.RegistrationAttendeeID))
                    {
                        var courseAttendees = course.Attendees.ToList();
                        courseAttendees.Add(new AttendeeModel()
                        {
                            RegistrationAttendeeId = attendee.RegistrationAttendeeID,
                            FirstName = attendee.RegAttendeeFirstName,
                            LastName = attendee.RegAttendeeLastName,
                            Email = attendee.RegAttendeeEmail,
                            Title = attendee.RegAttendeeTitle
                        });

                        course.Attendees = courseAttendees;
                    }
                }
            }

            return courses;
        }

        public static bool DisableUser(string memberKey)
        {
            var success = true;

            var member = ApplicationContext.Current.Services.MemberService.GetByKey(new Guid(memberKey));

            if (member != null)
            {
                member.SetValue("umbracoMemberApproved", false);
                try
                {
                    ApplicationContext.Current.Services.MemberService.Save(member);
                }
                catch
                {
                    success = false;
                }
            }
            else
            {
                success = false;
            }

            return success;

        }

        public static void UpdateUserTrainco(UserModel user,bool bCreate = true)
        {            
            using (var db = new americantraincoEntities())
            {
                try
                {
                    DateTime dtUTC = DateTime.UtcNow;
                    WebAccount account = new WebAccount();
                    if (!bCreate)
                    {
                        account = db.WebAccounts.Where(x => x.EmailAddress == user.Email).FirstOrDefault();
                        if (account == null)
                            return;
                    }
                    account.Updated = dtUTC;
                    account.EmailAddress = user.Email;
                    account.FirstName = user.FirstName;
                    account.LastName = user.LastName;
                    account.Title = user.Title;
                    account.PhoneNumber = user.Phone;
                    account.PhoneExtension = user.PhoneExtension;
                    if (bCreate)
                    {
                        account.Created = dtUTC;
                        db.WebAccounts.Add(account);
                    }
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                }
            }
        }

        public static string truncateString(this string str, int maxLength = 50)
        {
            return !string.IsNullOrEmpty(str) ? str.Substring(0, Math.Min(str.Length, maxLength)) : "";
        }

        public static void UpdateCompanyTrainco(string userEmail, CompanyModel company)
        {
            using (var db = new americantraincoEntities())
            {
                try
                {
                    WebAccount account = db.WebAccounts.Where(x => x.EmailAddress == userEmail).FirstOrDefault();
                    if (account == null)
                        return;
                    account.CompanyName = company.Name;
                    account.AddressLn1 = company.Address1.truncateString();
                    account.AddressLn2 = company.Address2.truncateString();
                    account.Country = company.Country.truncateString();
                    account.StateProvince = company.State.truncateString();
                    account.PostalCode = company.PostalCode.truncateString();
                    account.City = company.City.truncateString();
                    account.PriorCustomer = company.HasMakePreviousPurchase;
                    account.Industry = company.Industry;
                    account.OutsideTrainingFrequency = company.ExternalTrainingUsageAmount;
                    account.NbrEmplforTraining = company.NumberOfEmployees;
                    account.TrainingTopics = company.TrainingTopics;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
