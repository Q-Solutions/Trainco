using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
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

        public dynamic GetArloResponse()
        {
            string results = "<Result>NotFound</Result>";
            dynamic typeObj = null;
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
                    typeObj = ReadXMLSchema(results);
                }
                CacheObjects.SaveLog(_endpointAddress.ToString(), results);
            }
            catch (Exception ex)
            {

            }
            return typeObj;
        }

        public dynamic ReadXMLSchema(string ThisXml)
        {
            XDocument doc = XDocument.Parse(ThisXml);
            string jsonText = JsonConvert.SerializeXNode(doc);
            return JsonConvert.DeserializeObject<ExpandoObject>(jsonText);
            //T typeObj = Deserialize(doc);
            //return typeObj;
        }

        //private T Deserialize(XDocument data)
        //{
        //    if (data == null)
        //        return null;
        //    var ser = new XmlSerializer(typeof(T));
        //    return (T)ser.Deserialize(data.CreateReader());
        //}
    }
}
