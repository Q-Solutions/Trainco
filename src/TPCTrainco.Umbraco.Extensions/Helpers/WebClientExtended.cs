using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace TPCTrainco.Umbraco.Extensions.Helpers
{
    public class WebClientExtended : WebClient
    {
        public CookieContainer CookieContainer { get; set; }
        public Uri Uri { get; set; }
        public Dictionary<string, string> AsyncParams { get; set; }

        public WebClientExtended()
            : this(new CookieContainer())
        {
        }

        public WebClientExtended(CookieContainer cookies)
        {
            this.CookieContainer = cookies;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = this.CookieContainer;
            }
            HttpWebRequest httpRequest = (HttpWebRequest)request;
            httpRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            return httpRequest;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            CookieCollection cookies = ((HttpWebResponse)response).Cookies;
            String setCookieHeader = response.Headers[HttpResponseHeader.SetCookie];

            if (setCookieHeader != null)
            {
                //do something if needed to parse out the cookie.
                if (setCookieHeader != null)
                {
                    //create cookie
                    this.CookieContainer.Add(cookies);
                }
            }
            return response;
        }
    }
}