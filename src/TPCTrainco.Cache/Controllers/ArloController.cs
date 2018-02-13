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
    [RoutePrefix("api/arlo")]
    public class ArloController : ApiController
    {
        public StringBuilder DebugStr = new StringBuilder();
        public CacheMessage CacheMessage = new CacheMessage();
        public Dictionary<string, string> parentEvents = new Dictionary<string, string>();
        private string _endPoint = ConfigurationManager.AppSettings.Get("Caching:Arlo:Events:Url");
        private string _username = ConfigurationManager.AppSettings.Get("Caching:Arlo:Username");
        private string _password = ConfigurationManager.AppSettings.Get("Caching:Arlo:Password");
        private string _publicEndPoint = ConfigurationManager.AppSettings.Get("Caching:Arlo:PublicEvents:Url") + "&top=200&format=json";

        [HttpGet]
        public CacheMessage Index(string key)
        {
            try
            {
                if (key.ToLower() != ConfigurationManager.AppSettings.Get("Cache:ApiKey").ToLower())
                    throw new Exception("Invalid Key");
                SyncArloEvents();
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

        private void SyncArloEvents()
        {
            WebClient client = new WebClient();
            var response = client.DownloadString(ConfigurationManager.AppSettings.Get("Caching:Arlo:PublicEvents:Url") + "&top=1&includeTotalCount=true&format=json");
            if (string.IsNullOrEmpty(response))
                return;
            PublicEvents oObj = JsonConvert.DeserializeObject<PublicEvents>(response);
            if (oObj == null || oObj.TotalCount == 0)
                return;
            int totalCount = oObj.TotalCount;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < totalCount; i += 200)
            {
                tasks.Add(Task.Factory.StartNew((offset) => RunAsync(Convert.ToInt32(offset)), i));
            }
            Task.WaitAll(tasks.ToArray());
        }

        private void RunAsync(int eventOffset)
        {
            DebugStr.AppendLine("Task Offset: " + eventOffset);
            Dictionary<int, PublicEvent> publicEvents = GetPublicEvents(_publicEndPoint, eventOffset);
            if (publicEvents.Count == 0)
                return;
            List<int> eventIds = publicEvents.Keys.OrderBy(x => x).ToList();
            DebugStr.AppendLine("Public Events: " + string.Join(",", eventIds));
            List<Event> events = new List<Event>();
            events = GetEvents(events, eventIds);
            CacheObjects.SaveCourses(events, publicEvents);
        }

        private Dictionary<int, PublicEvent> GetPublicEvents(string endPoint, int offset)
        {
            Dictionary<int, PublicEvent> events = new Dictionary<int, PublicEvent>();
            WebClient client = new WebClient();
            var response = client.DownloadString(endPoint + "&skip=" + offset);
            if (string.IsNullOrEmpty(response))
                return events;
            PublicEvents oObj = JsonConvert.DeserializeObject<PublicEvents>(response);
            if (oObj == null)
                return events;
            return oObj.Items.ToDictionary(x => x.EventID, x => x);
        }

        private List<Event> GetEvents(List<Event> events, List<int> eventIds,string endpoint = null)
        {
            if(string.IsNullOrEmpty(endpoint))
                endpoint = _endPoint;
            ArloAPI api = new ArloAPI(endpoint, _username, _password);
            dynamic oEvents = api.GetArloResponse();
            if (oEvents == null || !((IDictionary<String, object>)oEvents).ContainsKey("Events"))
                return events;
            foreach (dynamic linkObj in oEvents.Events.Link)
            {
                if (linkObj == null)
                    continue;
                string rel = ((IDictionary<String, object>)linkObj)["@rel"].ToString();
                if (rel == "next")
                {
                    events = GetEvents(events, eventIds, ((IDictionary<String, object>)linkObj)["@href"].ToString());
                    break;
                }
                if (!((IDictionary<String, object>)linkObj).ContainsKey("Event"))
                    continue;
                Event oObj = ((ExpandoObject)linkObj.Event).ToObject<Event>();
                if (oObj == null || !eventIds.Contains(Convert.ToInt32(oObj.EventID)))
                    continue;
                foreach (dynamic link in oObj.Link)
                {
                    IDictionary<String, object> dict = (IDictionary<String, object>)link;
                    string href = dict["@href"].ToString();
                    if (!dict.ContainsKey("@title"))
                        continue;
                    string title = dict["@title"].ToString();
                    if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(href))
                        continue;
                    switch (title)
                    {
                        case "EventTemplate":
                            oObj.Template = GetEventTemplate(href);
                            break;
                        case "CustomFields":
                            oObj.CustomFields = GetCustomFields(href);
                            break;
                        case "AdvertisedRegions":
                            oObj.Region = GetEventRegion(href);
                            break;
                        case "Sessions":
                            oObj.Sessions = GetEventSessions(href);
                            break;
                        default:
                            break;
                    }
                }
                events.Add(oObj);
            }
            return events;
        }

        private List<Session> GetEventSessions(string href)
        {
            ArloAPI api = new ArloAPI(href, _username, _password);
            List<Session> sessions = new List<Session>();
            dynamic oObj = api.GetArloResponse();
            if (oObj == null || !((IDictionary<String, object>)oObj).ContainsKey("Sessions"))
                return sessions;
            foreach (dynamic link in oObj.Sessions.Link)
            {
                IDictionary<String, object> dict = (IDictionary<String, object>)link;
                if (!dict.ContainsKey("@title"))
                    continue;
                if (dict["@title"].ToString() == "EventSession")
                {
                    ArloAPI sessionApi = new ArloAPI(dict["@href"].ToString(), _username, _password);
                    dynamic sessionObj = sessionApi.GetArloResponse();
                    if (sessionObj == null || !((IDictionary<String, object>)sessionObj).ContainsKey("EventSession"))
                        continue;
                    sessions.Add(GetEventSession(sessionObj));
                }
            }
            return sessions;
        }

        private Session GetEventSession(dynamic oObj)
        {
            Session session = ((ExpandoObject)oObj.EventSession).ToObject<Session>();
            foreach (dynamic link in session.Link)
            {
                IDictionary<String, object> dict = (IDictionary<String, object>)link;
                string href = dict["@href"].ToString();
                if (!dict.ContainsKey("@title"))
                    continue;
                string title = dict["@title"].ToString();
                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(href))
                    continue;
                switch (title)
                {
                    case "VenueDetails":
                        session.Venue = GetSessionVenue(href);
                        break;
                    case "CustomFields":
                        session.CustomFields = GetCustomFields(href);
                        break;
                    case "Presenters":
                        session.Presenter = GetSessionPresenter(href);
                        break;
                    default:
                        break;
                }
            }
            return session;
        }

        private ArloContact GetSessionPresenter(string href)
        {
            ArloAPI api = new ArloAPI(href, _username, _password);
            dynamic oObj = api.GetArloResponse();
            ArloContact contact = null;
            if (oObj == null || !((IDictionary<String, object>)oObj).ContainsKey("Contacts"))
                return null;
            foreach (dynamic link in oObj.Contacts.Link)
            {
                try
                {
                    IDictionary<String, object> dict = (IDictionary<String, object>)link;
                    string linkHref = dict["@href"].ToString();
                    if (!dict.ContainsKey("@title"))
                        continue;
                    string title = dict["@title"].ToString();
                    if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(linkHref) || title != "Contact")
                        continue;
                    ArloAPI contactApi = new ArloAPI(linkHref, _username, _password);
                    dynamic contactObj = contactApi.GetArloResponse();
                    if (contactObj == null || !((IDictionary<String, object>)contactObj).ContainsKey("Contact"))
                        continue;
                    contact = ((ExpandoObject)contactObj.Contact).ToObject<ArloContact>();
                    break;
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            return contact;
        }

        private Venue GetSessionVenue(string href)
        {
            ArloAPI api = new ArloAPI(href, _username, _password);
            Venue venue = new Venue();
            dynamic oObj = api.GetArloResponse();
            if (oObj == null || !((IDictionary<String, object>)oObj).ContainsKey("EventSessionVenueDetails"))
                return venue;
            foreach (dynamic link in oObj.EventSessionVenueDetails.Link)
            {
                IDictionary<String, object> dict = (IDictionary<String, object>)link;
                if (!dict.ContainsKey("@title"))
                    continue;
                if (dict["@title"].ToString() == "Venue")
                {
                    ArloAPI venueApi = new ArloAPI(dict["@href"].ToString(), _username, _password);
                    dynamic venueObj = venueApi.GetArloResponse();
                    if (venueObj == null || !((IDictionary<String, object>)venueObj).ContainsKey("Venue"))
                        continue;
                    venue = ((ExpandoObject)venueObj.Venue).ToObject<Venue>();
                    foreach (dynamic venueLink in venue.Link)
                    {
                        IDictionary<String, object> venueDict = (IDictionary<String, object>)venueLink;
                        if (!venueDict.ContainsKey("@title"))
                            continue;
                        ArloAPI addressApi = new ArloAPI(venueDict["@href"].ToString(), _username, _password);
                        switch (venueDict["@title"].ToString())
                        {
                            case "PhysicalAddress":
                                dynamic addressoObj = addressApi.GetArloResponse();
                                if (addressoObj == null || !((IDictionary<String, object>)addressoObj).ContainsKey("Address"))
                                    continue;
                                venue.PhysicalAddress = ((ExpandoObject)addressoObj.Address).ToObject<Address>();
                                break;
                            case "FacilityInformation":
                                dynamic facilityObj = addressApi.GetArloResponse();
                                if (facilityObj == null || !((IDictionary<String, object>)facilityObj).ContainsKey("VenueFacilityInformation"))
                                    continue;
                                venue.Description = Convert.ToString(facilityObj.VenueFacilityInformation.Directions.Content);
                                break;
                            case "BookingContact":
                                dynamic contactObj = addressApi.GetArloResponse();
                                if (contactObj == null || !((IDictionary<String, object>)contactObj).ContainsKey("Contact"))
                                    continue;
                                venue.BookingContact = ((ExpandoObject)contactObj.Contact).ToObject<ArloContact>();
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                }
            }
            return venue;
        }

        private Region GetEventRegion(string href)
        {
            ArloAPI api = new ArloAPI(href, _username, _password);
            Region region = null;
            dynamic oObj = api.GetArloResponse();
            if (oObj == null || !((IDictionary<String, object>)oObj).ContainsKey("Regions"))
                return region;
            foreach (dynamic link in oObj.Regions.Link)
            {
                IDictionary<String, object> dict = (IDictionary<String, object>)link;
                if (!dict.ContainsKey("@title"))
                    continue;
                if (dict["@title"].ToString() == "Region")
                {
                    ArloAPI regionApi = new ArloAPI(dict["@href"].ToString(), _username, _password);
                    dynamic regionObj = regionApi.GetArloResponse();
                    if (regionObj == null || !((IDictionary<String, object>)regionObj).ContainsKey("Region"))
                        continue;
                    region = ((ExpandoObject)regionObj.Region).ToObject<Region>();
                    break;
                }
            }
            return region;
        }

        private EventTemplate GetEventTemplate(string href)
        {
            ArloAPI api = new ArloAPI(href, _username, _password);
            dynamic oObj = api.GetArloResponse();
            if (oObj == null || !((IDictionary<String, object>)oObj).ContainsKey("EventTemplate"))
                return null;
            return ((ExpandoObject)oObj.EventTemplate).ToObject<EventTemplate>();
        }

        private Dictionary<string, string> GetCustomFields(string href)
        {
            Dictionary<string, string> customFields = new Dictionary<string, string>();
            ArloAPI api = new ArloAPI(href, _username, _password);
            dynamic oObj = api.GetArloResponse();
            if (oObj == null || !((IDictionary<String, object>)oObj).ContainsKey("CustomFields"))
                return customFields;
            IDictionary<String, object> dict = (IDictionary<String, object>)oObj.CustomFields;
            if (!dict.ContainsKey("Field"))
                return customFields;
            List<object> fields = new List<object>();
            if (dict["Field"].GetType() == typeof(ExpandoObject))
                fields.Add(dict["Field"]);
            else
                fields = dict["Field"] as List<object>;
            foreach (dynamic field in fields)
            {
                IDictionary<String, object> fieldDict = (IDictionary<String, object>)field;
                if (!fieldDict.ContainsKey("Name") || !fieldDict.ContainsKey("Value") || !fieldDict.ContainsKey("Link"))
                    continue;
                IDictionary<String, object> fieldDescDict = (IDictionary<String, object>)field.Link;
                if (!fieldDescDict.ContainsKey("@title") || fieldDescDict["@title"].ToString() != "FieldDescription" || !fieldDescDict.ContainsKey("@href"))
                    continue;
                string fieldDesc = getCustomFieldDescription(fieldDescDict["@href"].ToString());
                IDictionary<String, object> fieldValueDict = (IDictionary<String, object>)field.Value;
                if (string.IsNullOrEmpty(fieldDesc) || !fieldValueDict.ContainsKey(fieldDesc) || fieldValueDict[fieldDesc] == null)
                    continue;
                customFields.Add(field.Name.ToString(), fieldValueDict[fieldDesc].ToString());
            }
            return customFields;
        }

        private string getCustomFieldDescription(string href)
        {
            string field = "";
            ArloAPI api = new ArloAPI(href, _username, _password);
            dynamic oObj = api.GetArloResponse();
            if (oObj == null || !((IDictionary<String, object>)oObj).ContainsKey("FieldDescription"))
                return field;
            field = oObj.FieldDescription.ValueType.TypeName;
            if (Convert.ToBoolean(oObj.FieldDescription.ValueType.IsArray))
                field = "";
            return field;
        }
    }
}
