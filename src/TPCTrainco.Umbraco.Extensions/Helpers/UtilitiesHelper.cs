using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TPCTrainco.Umbraco.Extensions.Helpers
{
    public static class UtilitiesHelper
    {
        public static string GetClientIpAddress(HttpRequestBase request)
        {
            string ip = request.Headers["X-Forwarded-For"]; // AWS compatibility

            if (string.IsNullOrEmpty(ip))
            {
                ip = request.UserHostAddress;
            }

            return ip;
        }

        public static string GetClientIpAddress(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return GetClientIpAddress(((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request);
            }
            else if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                RemoteEndpointMessageProperty prop;
                prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
                return prop.Address;
            }
            else
            {
                return null;
            }
        }

        public static void LogException(Exception exception,string path)
        {
            System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
            System.IO.StreamWriter s = new System.IO.StreamWriter(fs);
            s.BaseStream.Seek(0, System.IO.SeekOrigin.End);
            s.WriteLine("=============================================================================================================");
            s.WriteLine("-------------------------------------------------------------------------------------------------------------");
            s.WriteLine("ERROR DATE \t: " + System.DateTime.UtcNow.ToString(System.Globalization.CultureInfo.InvariantCulture));
            s.WriteLine("-------------------------------------------------------------------------------------------------------------");
            s.WriteLine("ERROR MESSAGE \t: " + exception.Message);
            s.WriteLine("-------------------------------------------------------------------------------------------------------------");
            s.WriteLine("INNER EXCEPTION \t: " + exception.InnerException.ToString());
            s.WriteLine("-------------------------------------------------------------------------------------------------------------");
            s.WriteLine("SOURCE \t: " + exception.Source);
            s.WriteLine("-------------------------------------------------------------------------------------------------------------");
            s.WriteLine("FORM NAME \t: " + System.Web.HttpContext.Current.Request.Url.ToString());
            s.WriteLine("-------------------------------------------------------------------------------------------------------------");
            s.WriteLine("TARGETSITE \t: " + exception.TargetSite.ToString());
            s.WriteLine("-------------------------------------------------------------------------------------------------------------");
            s.WriteLine("STACKTRACE \t: " + exception.StackTrace + System.Diagnostics.EventLogEntryType.Error);
            s.WriteLine("-------------------------------------------------------------------------------------------------------------");
            s.WriteLine("=============================================================================================================");
            s.Close();
        } 
    }
}
