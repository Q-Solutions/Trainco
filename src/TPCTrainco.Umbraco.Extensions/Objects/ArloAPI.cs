using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using TPCTrainco.Umbraco.Extensions.Helpers;

namespace TPCTrainco.Umbraco.Extensions.Objects
{
    public class ArloAPI
    {
        private CookieContainer cookieContainer;
        private EndpointAddress _endpointAddress;
        private string logon { get; set; }
        private string password { get; set; }

        public ArloAPI(EndpointAddress endpointAddress)
        {
            this._endpointAddress = endpointAddress;
        }

        public ArloAPI(string Uri,string username, string password)
        {
            EndpointAddress endpointAddress = new EndpointAddress(Uri);
            this._endpointAddress = endpointAddress;
            this.logon = username;
            this.password = password;
        }

        public T GetArloResponce<T>() where T : class, new()
        {
            string results = "<Result>NotFound</Result>";
            T typeObj = null;
            try
            {
                using (WebClientExtended webEx = new WebClientExtended())
                {
                    webEx.CookieContainer = cookieContainer = new CookieContainer();
                    webEx.Headers[HttpRequestHeader.Accept] = "application/xml";
                    webEx.Headers[HttpRequestHeader.ContentType] = "application/xml";
                    NetworkCredential networkCredential = new NetworkCredential(logon, password); // logon in format "domain\username"
                    CredentialCache myCredentialCache = new CredentialCache {{_endpointAddress.Uri, "Basic", networkCredential}};
                    webEx.Credentials = myCredentialCache;
                    webEx.QueryString.Clear();
                    webEx.Encoding = Encoding.UTF8;
                    results = webEx.DownloadString(new Uri(_endpointAddress.ToString(), UriKind.Absolute));
                    typeObj = ReadXMLSchema<T>(results);
                }
            }
            catch (Exception ex)
            {

            }
            return typeObj;
        }

        public T ReadXMLSchema<T>(string ThisXml) where T : class, new()
        {
            XDocument doc = XDocument.Parse(ThisXml);
            T typeObj = Deserialize<T>(doc);
            return typeObj;
        }

        private T Deserialize<T>(XDocument data) where T : class, new()
        {
            if (data == null)
                return null;
            var ser = new XmlSerializer(typeof(T));
            return (T)ser.Deserialize(data.CreateReader());
        }
    }
}
