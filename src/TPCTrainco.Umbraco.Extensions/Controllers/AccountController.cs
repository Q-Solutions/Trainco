using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Security;
using TPCTrainco.Umbraco.Extensions.Attributes;
using TPCTrainco.Umbraco.Extensions.Helpers;
using TPCTrainco.Umbraco.Extensions.Models.Account;
using TPCTrainco.Umbraco.Extensions.Models.API.Request;
using TPCTrainco.Umbraco.Extensions.Models.API.Response;
using TPCTrainco.Umbraco.Extensions.Objects;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace TPCTrainco.Umbraco.Extensions.Controllers
{
    // ~/api/Account/<method>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AccountController : ApiController
    {
        #region HttpPost

        /// <summary>
        /// Logs a user in and return an auth token.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage Login(LoginRequestModel request)
        {
            var authToken = String.Empty;

            if (request != null && request.Username != null && request.Password != null && Membership.ValidateUser(request.Username, request.Password))
            {
                var member = ApplicationContext.Current.Services.MemberService.GetByUsername(request.Username);

                var host = Request.RequestUri.Host;

                var expiration = request.PersistantLogin ? DateTime.UtcNow.AddYears(100).Ticks : DateTime.UtcNow.Ticks;

                authToken = AccountHelper.GenerateToken(member.Key.ToString(), member.Email, Request.RequestUri.Host, UtilitiesHelper.GetClientIpAddress(Request), Request.Headers.UserAgent.ToString(), expiration);
            }

            var responseModel = new LoginResponseModel()
            {
                Status = String.IsNullOrEmpty(authToken) ? System.Net.HttpStatusCode.Unauthorized.ToString() : System.Net.HttpStatusCode.OK.ToString(),
                StatusCode = String.IsNullOrEmpty(authToken) ? System.Net.HttpStatusCode.Unauthorized : System.Net.HttpStatusCode.OK,
                Result = authToken
            };

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }

        /// <summary>
        /// Sends a password reset email to the user.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage ForgottenPassword(ForgottenPasswordRequestModel request)
        {
            var success = AccountHelper.ForgottenPassword(request.Email, Request.RequestUri);

            return Request.CreateResponse(success ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Create a new user account
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage CreateUser(CreateUserRequestModel request)
        {
            var exists = ApplicationContext.Current.Services.MemberService.Exists(request.User.Email);
            CreateUserResponseModel responseModel = null;

            if (false == exists)
            {
                var user = AccountHelper.CreateUser(request.User, Request.RequestUri);

                if (!String.IsNullOrEmpty(user.Key))
                {
                    if (!String.IsNullOrEmpty(request.Company.Username) && !request.Company.Username.Equals(request.User.Email))
                    {
                        request.Company = AccountHelper.GetCompany(request.Company.Username);
                    }

                    var success = AccountHelper.UpdateCompany(user.Key, request.Company);
                }
                if (user != null && !String.IsNullOrEmpty(user.ValidationCode))
                    AccountHelper.CreateUserTrainco(request.User, request.Company);
                responseModel = new CreateUserResponseModel()
                {
                    Status = String.IsNullOrEmpty(user.ValidationCode) ? System.Net.HttpStatusCode.NotModified.ToString() : System.Net.HttpStatusCode.Created.ToString(),
                    StatusCode = String.IsNullOrEmpty(user.ValidationCode) ? System.Net.HttpStatusCode.NotModified : System.Net.HttpStatusCode.Created,
                    Result = user != null
                };
            }
            else
            {
                responseModel = new CreateUserResponseModel()
                {
                    Status = System.Net.HttpStatusCode.NotModified.ToString(),
                    StatusCode = System.Net.HttpStatusCode.NotModified,
                    Result = false
                };
            }

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }


        /// <summary>
        /// Activates a user account and logs the user in.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage EmailVerification(EmailVerificationRequestModel request)
        {
            var authToken = String.Empty;

            IMember member = null;

            try
            {
                if (AccountHelper.EmailVerification(request.Email, request.ValidationCode, out member))
                {
                    authToken = AccountHelper.GenerateToken(member.Key.ToString(), member.Email, Request.RequestUri.Host, UtilitiesHelper.GetClientIpAddress(Request), Request.Headers.UserAgent.ToString(), DateTime.UtcNow.Ticks);
                }
                else
                {
                    LogHelper.Warn(typeof(AccountController), "Email Verification: Invalid verification: " + request.Email);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(AccountController), "Email Verification: Error on verification", ex);
            }

            var responseModel = new LoginResponseModel()
            {
                Status = String.IsNullOrEmpty(authToken) ? System.Net.HttpStatusCode.Unauthorized.ToString() : System.Net.HttpStatusCode.Accepted.ToString(),
                StatusCode = String.IsNullOrEmpty(authToken) ? System.Net.HttpStatusCode.Unauthorized : System.Net.HttpStatusCode.Accepted,
                Result = authToken
            };

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }


        /// <summary>
        /// Sends and email with course info to the provided email
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TokenAuthorization]
        public HttpResponseMessage ShareCourse(ShareCourseRequestModel request)
        {
            var memberKey = AccountHelper.GetMemberKeyFromToken(Request.Headers.Authorization.Parameter);

            AccountHelper.ShareCourse(memberKey, request.Email, request.CourseId, Request.RequestUri);

            return Request.CreateResponse(System.Net.HttpStatusCode.OK);
        }

        [HttpPost]
        [TokenAuthorization]
        public HttpResponseMessage Logout()
        {
            //var memberKey = AccountHelper.GetMemberKeyFromToken(Request.Headers.Authorization.Parameter);

            Users.Remove(HttpContext.Current.Session);

            return Request.CreateResponse(System.Net.HttpStatusCode.OK);
        }

        #endregion


        #region HttpGet

        [HttpGet]
        public HttpResponseMessage GetNavigation()
        {
            List<NavigationItem> navigation = new List<NavigationItem>();

            var homePage = UmbracoContext.Current.ContentCache.GetAtRoot().FirstOrDefault(r => r.DocumentTypeAlias == "HomePage");

            if (homePage != null)
            {
                var menuItems = homePage.Children.Where("navigationTop == true");
                foreach (IPublishedContent item in homePage.Children.Where("navigationTop == true").OrderBy("SortOrder"))
                {
                    if (item.DocumentTypeAlias == "SeminarMainOverview")
                    {
                        string navName = item.GetPropertyValue<string>("navigationTitle", item.Name);

                        var navItem = new NavigationItem()
                        {
                            Name = item.GetPropertyValue<string>("navigationTitle", item.Name),
                            Url = item.Url,
                            Children = new List<NavigationItem>()
                        };

                        foreach (IPublishedContent subItem in item.Children)
                        {
                            navItem.Children.Add(new NavigationItem()
                            {
                                Name = subItem.GetPropertyValue<string>("navigationTitle", subItem.Name),
                                Url = subItem.Url,
                                Children = new List<NavigationItem>()
                            });
                        }
                        navigation.Add(navItem);
                    }
                    else
                    {
                        navigation.Add(new NavigationItem()
                        {
                            Name = item.GetPropertyValue<string>("navigationTitle", item.Name),
                            Url = item.Url,
                            Children = new List<NavigationItem>()
                        });
                    }
                }
            }

            return Request.CreateResponse(System.Net.HttpStatusCode.OK, navigation);
        }

        /// <summary>
        /// Check if the user has already registered.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage UserExists([FromUri] UserExistsRequestModel request)
        {
            var exists = ApplicationContext.Current.Services.MemberService.Exists(request.Email);

            if (true == exists)
            {
                var member = ApplicationContext.Current.Services.MemberService.GetByEmail(request.Email);

                if (false == member.IsApproved)
                {
                    LogHelper.Info(typeof(AccountHelper), "User Exists: Member not approved. Deleting: " + request.Email);

                    ApplicationContext.Current.Services.MemberService.Delete(member);

                    exists = false;
                }
            }

            var responseModel = new UserExistsResponseModel()
            {
                Status = exists ? System.Net.HttpStatusCode.BadRequest.ToString() : System.Net.HttpStatusCode.OK.ToString(),
                StatusCode = exists ? System.Net.HttpStatusCode.BadRequest : System.Net.HttpStatusCode.OK,
                Result = exists
            };

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }

        /// <summary>
        /// Check if the provided email address has take a course before and if true returns the supervisors company info.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage HasTakenCourse([FromUri] HasTakenCourseRequestModel request)
        {
            var supervisorMemberKey = AccountHelper.GetSupervisorMemberKey(request.Email);

            CompanyModel company = null;

            if (!String.IsNullOrEmpty(supervisorMemberKey))
            {
                company = AccountHelper.GetCompany(supervisorMemberKey);
            }

            var responseModel = new HasTakenCourseResponseModel()
            {
                Status = company == null ? System.Net.HttpStatusCode.BadRequest.ToString() : System.Net.HttpStatusCode.OK.ToString(),
                StatusCode = company == null ? System.Net.HttpStatusCode.BadRequest : System.Net.HttpStatusCode.OK,
                Result = company
            };

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }

        /// <summary>
        /// Get the clients IP address
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetClientIpAddress()
        {
            var ip = UtilitiesHelper.GetClientIpAddress(Request);

            var responseModel = new GetClientIpAddressResponseModel()
            {
                Status = System.Net.HttpStatusCode.Accepted.ToString(),
                StatusCode = System.Net.HttpStatusCode.Accepted,
                Result = ip
            };

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }

        /// <summary>
        /// Get user profile info
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [TokenAuthorization]
        public HttpResponseMessage GetUser()
        {
            var memberKey = AccountHelper.GetMemberKeyFromToken(Request.Headers.Authorization.Parameter);

            var user = AccountHelper.GetUser(memberKey);

            var responseModel = new GetUserResponseModel()
            {
                Status = System.Net.HttpStatusCode.OK.ToString(),
                StatusCode = System.Net.HttpStatusCode.OK,
                Result = user
            };

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }

        /// <summary>
        /// Get the users company info
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [TokenAuthorization]
        public HttpResponseMessage GetCompany()
        {
            var memberKey = AccountHelper.GetMemberKeyFromToken(Request.Headers.Authorization.Parameter);

            var company = AccountHelper.GetCompany(memberKey);

            var responseModel = new GetCompanyResponseModel()
            {
                Status = System.Net.HttpStatusCode.OK.ToString(),
                StatusCode = System.Net.HttpStatusCode.OK,
                Result = company
            };

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }

        /// <summary>
        /// Get the memebers billing information
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [TokenAuthorization]
        public HttpResponseMessage GetBilling()
        {
            var memberKey = AccountHelper.GetMemberKeyFromToken(Request.Headers.Authorization.Parameter);

            var billing = AccountHelper.GetBilling(memberKey);

            var responseModel = new GetBillingResponseModel()
            {
                Status = System.Net.HttpStatusCode.OK.ToString(),
                StatusCode = System.Net.HttpStatusCode.OK,
                Result = billing
            };

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }

        /// <summary>
        /// Get the list of save courses for the user.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [TokenAuthorization]
        public HttpResponseMessage GetSaveForLater()
        {
            var memberKey = AccountHelper.GetMemberKeyFromToken(Request.Headers.Authorization.Parameter);

            var courses = AccountHelper.GetSaveForLater(memberKey);

            var responseModel = new GetSaveForLaterResponseModel()
            {
                Status = System.Net.HttpStatusCode.OK.ToString(),
                StatusCode = System.Net.HttpStatusCode.OK,
                Result = courses
            };

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }

        /// <summary>
        /// Get all upcoming courses that the user has registered for
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [TokenAuthorization]
        public HttpResponseMessage GetUpcomingCourses()
        {
            var memberKey = AccountHelper.GetMemberKeyFromToken(Request.Headers.Authorization.Parameter);
            var memberEmail = AccountHelper.GetEmailFromToken(Request.Headers.Authorization.Parameter);

            var courses = AccountHelper.GetUpcomingCourses(memberKey, memberEmail);

            var responseModel = new GetUpcomingEventsResponseModel()
            {
                Status = System.Net.HttpStatusCode.OK.ToString(),
                StatusCode = System.Net.HttpStatusCode.OK,
                Result = courses
            };

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }


        /// <summary>
        /// Get all past courses that the user has registered for
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [TokenAuthorization]
        public HttpResponseMessage GetPastCourses()
        {
            var memberKey = AccountHelper.GetMemberKeyFromToken(Request.Headers.Authorization.Parameter);
            var memberEmail = AccountHelper.GetEmailFromToken(Request.Headers.Authorization.Parameter);

            var courses = AccountHelper.GetPastCourses(memberKey, memberEmail);

            var responseModel = new GetPastEventsResponseModel()
            {
                Status = System.Net.HttpStatusCode.OK.ToString(),
                StatusCode = System.Net.HttpStatusCode.OK,
                Result = courses
            };

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }

        #endregion


        #region HttpPut

        /// <summary>
        /// Updates the users password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public HttpResponseMessage UpdatePassword(ResetPasswordRequestModel request)
        {
            var authToken = String.Empty;

            IMember member = null;

            if (String.IsNullOrEmpty(request.ValidationCode) && AccountHelper.IsTokenValid(Request.Headers.Authorization.Parameter, Request.RequestUri.Host, UtilitiesHelper.GetClientIpAddress(Request), Request.Headers.UserAgent.ToString()))
            {
                request.ValidationCode = AccountHelper.GetMemberKeyFromToken(Request.Headers.Authorization.Parameter);
            }

            if (AccountHelper.UpdatePassword(request.Email, request.Password, request.ValidationCode, out member))
            {
                authToken = AccountHelper.GenerateToken(member.Key.ToString(), member.Email, Request.RequestUri.Host, UtilitiesHelper.GetClientIpAddress(Request), Request.Headers.UserAgent.ToString(), DateTime.UtcNow.Ticks);
            }

            var responseModel = new ResetPasswordResponseModel()
            {
                Status = String.IsNullOrEmpty(authToken) ? System.Net.HttpStatusCode.Unauthorized.ToString() : System.Net.HttpStatusCode.Accepted.ToString(),
                StatusCode = String.IsNullOrEmpty(authToken) ? System.Net.HttpStatusCode.Unauthorized : System.Net.HttpStatusCode.Accepted,
                Result = authToken
            };

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }

        /// <summary>
        /// Updated the users company info
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [TokenAuthorization]
        public HttpResponseMessage UpdateCompany(UpdateCompanyRequestModel request)
        {
            var memberKey = AccountHelper.GetMemberKeyFromToken(Request.Headers.Authorization.Parameter);

            var success = AccountHelper.UpdateCompany(memberKey, request.Company);

            var responseModel = new UpdateCompanyResponseModel()
            {
                Status = System.Net.HttpStatusCode.Accepted.ToString(),
                StatusCode = System.Net.HttpStatusCode.Accepted,
                Success = success,
                Result = request.Company
            };

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }

        /// <summary>
        /// Update the users billing info
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [TokenAuthorization]
        public HttpResponseMessage UpdateBilling(UpdateBillingRequestModel request)
        {
            var memberKey = AccountHelper.GetMemberKeyFromToken(Request.Headers.Authorization.Parameter);

            var success = AccountHelper.UpdateBilling(memberKey, request.Billing);

            var responseModel = new UpdateBillingResponseModel()
            {
                Status = System.Net.HttpStatusCode.Accepted.ToString(),
                StatusCode = System.Net.HttpStatusCode.Accepted,
                Result = request.Billing,
                Success = success
            };

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }

        /// <summary>
        /// Updates the users profile info
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [TokenAuthorization]
        public HttpResponseMessage UpdateUser(UpdateUserRequestModel request)
        {
            var memberKey = AccountHelper.GetMemberKeyFromToken(Request.Headers.Authorization.Parameter);

            var authToken = String.Empty;

            IMember member = null;

            if (AccountHelper.UpdateUser(memberKey, request.User, out member))
            {
                authToken = AccountHelper.GenerateToken(member.Key.ToString(), member.Email, Request.RequestUri.Host, UtilitiesHelper.GetClientIpAddress(Request), Request.Headers.UserAgent.ToString(), DateTime.UtcNow.Ticks);
            }

            var responseModel = new UpdateUserResponseModel()
            {
                Status = System.Net.HttpStatusCode.Accepted.ToString(),
                StatusCode = System.Net.HttpStatusCode.Accepted,
                Success = String.IsNullOrEmpty(authToken),
                AuthToken = authToken,
                Result = request.User
            };

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }

        /// <summary>
        /// Update the saved list of courses for the user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [TokenAuthorization]
        public HttpResponseMessage UpdateSaveForLater(UpdateSaveForLaterRequestModel request)
        {
            var memberKey = AccountHelper.GetMemberKeyFromToken(Request.Headers.Authorization.Parameter);

            var success = AccountHelper.UpdateSaveForLater(memberKey, request.CourseId);

            return Request.CreateResponse(success ? System.Net.HttpStatusCode.Accepted : System.Net.HttpStatusCode.NotModified);
        }

        [HttpPut]
        [TokenAuthorization]
        public HttpResponseMessage DisableAccount()
        {
            var memberKey = AccountHelper.GetMemberKeyFromToken(Request.Headers.Authorization.Parameter);

            var success = AccountHelper.DisableUser(memberKey);

            return Request.CreateResponse(success ? System.Net.HttpStatusCode.Accepted : System.Net.HttpStatusCode.NotModified);
        }

        #endregion


        #region HttpDelete

        /// <summary>
        /// Deletes a course from the save list of courses for the user.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete]
        [TokenAuthorization]
        public HttpResponseMessage DeleteSaveForLater(DeleteSaveForLaterRequestModel request)
        {
            var memberKey = AccountHelper.GetMemberKeyFromToken(Request.Headers.Authorization.Parameter);

            var success = AccountHelper.DeleteSaveForLater(memberKey, request.CourseId);

            var responseModel = new DeleteSaveForLaterResponseModel()
            {
                Status = System.Net.HttpStatusCode.Accepted.ToString(),
                StatusCode = System.Net.HttpStatusCode.Accepted,
                Result = success
            };

            return Request.CreateResponse(responseModel.StatusCode, responseModel);
        }

        #endregion
    }
}
