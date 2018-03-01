using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using TPCTrainco.Umbraco.Extensions.Objects;
using TPCTrainco.Cache.Extension;
using System.Threading.Tasks;

namespace TPCTrainco.Cache.Controllers
{
  [RoutePrefix("api/ArloUpdateApi")]
  public class ArloUpdateApiController : ApiController 
  {
    public StringBuilder DebugStr = new StringBuilder();  
    public CacheMessage CacheMessage = new CacheMessage();
    private string _endPointVenue = ConfigurationManager.AppSettings.Get("Caching:Arlo:PublicVenues:Url");
    private string _username = ConfigurationManager.AppSettings.Get("Caching:Arlo:Username");
    private string _password = ConfigurationManager.AppSettings.Get("Caching:Arlo:Password");



    [HttpGet]
    public CacheMessage Index(string key)
    {
      try
      {
        if (key.ToLower() != ConfigurationManager.AppSettings.Get("Cache:ApiKey").ToLower())
          throw new Exception("Invalid Key");
        ArloUpdateVenue();
        DebugStr.AppendLine("ALL DONE");
        CacheMessage.Success = true;
        CacheMessage.Message = DebugStr.ToString();
      }
      catch (Exception ex)
      {
        CacheMessage.Message = DebugStr.ToString() + "\r\n\r\n" + ex.ToString();
      }
      return CacheMessage;

    }

    public void ArloUpdateVenue(string endpoint = null)
    {
      string lastSyncdate = CacheObjects.GetLastSyncDate();
      if (string.IsNullOrEmpty(endpoint))
        endpoint = _endPointVenue + "(%27" + string.Join(",", lastSyncdate) + "T00:00:00.000Z%27)" + "&expand=Venue";
      ArloAPI api = new ArloAPI(endpoint, _username, _password);
      dynamic oVenues = api.GetArloResponse();
      List<Venue> Venues = new List<Venue>();
      Venues = GetVenues(Venues);
      CacheObjects.SaveVenues(Venues);
    }
    public List<Venue> GetVenues(List<Venue> Venues, string endpoint = null)
    {
      string lastSyncdate = CacheObjects.GetLastSyncDate();
      if (string.IsNullOrEmpty(endpoint))
        endpoint = _endPointVenue + "(%27" + string.Join(",", lastSyncdate) + "T00:00:00.000Z%27)" + "&expand=Venue";
      ArloAPI api = new ArloAPI(endpoint, _username, _password);
      dynamic oVenues = api.GetArloResponse();
      if (oVenues == null || !((IDictionary<string,Object>)oVenues).ContainsKey("Venues"))
         return Venues;
      foreach (dynamic LinkObj in oVenues.Venues.Link)
      {
        if (LinkObj == null)
          continue;
        string rel = ((IDictionary<string, object>)LinkObj)["@rel"].ToString();
        if (rel == "next")
        {
          Venues = GetVenues(Venues, ((IDictionary<string, object>)LinkObj)["@href"].ToString());
          break;
        }
        if (!((IDictionary<string, object>)LinkObj).ContainsKey("Venue"))
          continue;
        Venue oObj = ((ExpandoObject)LinkObj.Venue).ToObject<Venue>();
        foreach(dynamic link in oObj.Link)
        {
          try
          {
            IDictionary<string, object> dict = (IDictionary<string, Object>)link;
            if (!dict.ContainsKey("@title"))
              continue;
            if (dict["@title"].ToString() == "BookingContact")
            {
              ArloAPI BookConApi = new ArloAPI(dict["@href"].ToString(), _username, _password);
              dynamic BookConObj = BookConApi.GetArloResponse();
              if (BookConObj == null || !((IDictionary<string, object>)BookConObj).ContainsKey("Contact"))
                continue;
              oObj.BookingContact = ((ExpandoObject)BookConObj.Contact).ToObject<ArloContact>();
            }
            if (dict["@title"].ToString() == "TimeZone")
            {
              ArloAPI TimeZoneApi = new ArloAPI(dict["@href"].ToString(), _username, _password);
              dynamic TimeZoneObj = TimeZoneApi.GetArloResponse();
              if (TimeZoneObj == null || !((IDictionary<string, object>)TimeZoneObj).ContainsKey("TimeZone"))
                continue;
              oObj.Timezone = ((ExpandoObject)TimeZoneObj.TimeZone).ToObject<TimeZones>();
            }
          }
          catch(Exception ex)
          {
            
          }
        }
        Venues.Add(oObj);
      }
      return Venues;
    }

  
  }

  
}

