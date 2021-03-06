﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TPCTrainco.Umbraco.Extensions.Objects
{
    public static class RegCookies
    {
        public static string CookieName = "TPC-Trainco-Reg";
        public static string CookieValue = "TPC-Reg";
        public static string EncryptKey = "JcTGn7cYNf^U(6fd0n%T7FD7uof6t5nd(&Tfdtnsa(fdyus9id&";

        public static void Set(string regId)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[CookieName] ?? new HttpCookie(CookieName);

            cookie.Values[CookieValue] = EncryptRegId(regId);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static string Get()
        {
            string output = null;

            try
            {

                HttpCookie cookie = HttpContext.Current.Request.Cookies[CookieName];
                if (cookie != null)
                {
                    string cookieValue = cookie.Values[CookieValue];

                    if (false == string.IsNullOrWhiteSpace(cookieValue))
                    {
                        output = DecryptRegId(cookieValue);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return output;
        }


        public static string EncryptRegId(string regId)
        {
            return EncryptString.Encrypt(regId, EncryptKey);
        }


        public static string DecryptRegId(string regIdEncrypted)
        {
            return EncryptString.Decrypt(regIdEncrypted, EncryptKey);
        }


        public static string Remove()
        {
            string output = null;

            try
            {

                HttpCookie cookie = HttpContext.Current.Request.Cookies[CookieName];
                if (cookie != null)
                {
                    cookie.Expires = DateTime.Now.AddDays(-1);
                    HttpContext.Current.Response.Cookies.Add(cookie);
                }
            }
            catch (Exception ex)
            {

            }

            return output;
        }
    }



}
