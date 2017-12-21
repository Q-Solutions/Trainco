using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Text;
using System.Diagnostics;
using TPCTrainco.Umbraco.Extensions.Models;
using System.Configuration;
using System.Runtime.Caching;
using TPCTrainco.Umbraco.Extensions;
using TPCTrainco.Umbraco.Extensions.Objects;
using System.Linq;
using Umbraco.Web;
using Umbraco.Core.Models;
using Newtonsoft.Json;
using Umbraco.Core.Persistence;
using Umbraco.Core;
using System.Net;
using System.ServiceModel;
using Umbraco.Core.Services;
using System.Dynamic;
using TPCTrainco.Cache.Extension;

namespace TPCTrainco.Cache.Controllers
{
    [RoutePrefix("api/cache")]
    public class CacheController : ApiController
    {
        public StringBuilder DebugStr = new StringBuilder();
        public CacheMessage CacheMessage = new CacheMessage();


        // GET api/cache/update
        [HttpGet]
        public CacheMessage Index(string key)
        {
            logCacheLog();
            SyncArloEvents();
            string apiDomain = ConfigurationManager.AppSettings.Get("Cache:UmbracoCourseApiDomain");
            DebugApp("-= CACHE PROCESS START =-", ref DebugStr);
            DebugApp("", ref DebugStr);

            CacheMessage.Success = false;

            if (key.ToLower() != ConfigurationManager.AppSettings.Get("Cache:ApiKey").ToLower())
            {
                CacheMessage.Message = "Invalid Key";
                return CacheMessage;
            }

            try
            {
                ProcessCourses();
                ProcessSeminars();

                DebugApp("Contact Website to Refresh Cache...", ref DebugStr);

                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(string.Format(apiDomain + "/api/cacheupdate/refresh"));
                webReq.Method = "GET";
                HttpWebResponse webResponse = (HttpWebResponse)webReq.GetResponse();
                DebugApp("Done.", ref DebugStr);

                DebugApp("", ref DebugStr);
                DebugApp("-= ALL DONE! =-", ref DebugStr);

                CacheMessage.Success = true;
                CacheMessage.Message = DebugStr.ToString();


            }
            catch (Exception ex)
            {
                CacheMessage.Message = DebugStr.ToString() + "\r\n\r\n" + ex.ToString();
            }

            return CacheMessage;
        }

        [HttpGet]
        public CacheMessage LocationLookup(string key2)
        {
            DebugApp("-= CACHE PROCESS START =-", ref DebugStr);
            DebugApp("", ref DebugStr);

            CacheMessage.Success = false;

            if (key2.ToLower() != ConfigurationManager.AppSettings.Get("Cache:ApiKey").ToLower())
            {
                CacheMessage.Message = "Invalid Key";
                return CacheMessage;
            }

            try
            {
                List<StateLookup> stateLookupList = new List<StateLookup>();
                string line = null;

                System.IO.StreamReader fileState = new System.IO.StreamReader("C:\\_Repo\\TPCTrainco\\src\\TPCTrainco.Umbraco\\state_table.csv");
                while ((line = fileState.ReadLine()) != null)
                {
                    List<string> lineArray = line.Split(',').ToList();

                    if (lineArray != null && lineArray.Count == 5)
                    {
                        int regionCode = 0;

                        if (false == string.IsNullOrWhiteSpace(lineArray[4]))
                        {
                            regionCode = Convert.ToInt32(lineArray[4]);
                        }

                        stateLookupList.Add(new StateLookup { Id = lineArray[0], Name = lineArray[1], Abbreviation = lineArray[2], Country = lineArray[3], RegionCode = regionCode });
                    }
                }
                fileState.Close();




                // Read the file and display it line by line.
                System.IO.StreamReader file = new System.IO.StreamReader("C:\\_Repo\\TPCTrainco\\src\\TPCTrainco.Umbraco\\worldcitiespop.txt");
                while ((line = file.ReadLine()) != null)
                {
                    List<string> lineArray = line.Split(',').ToList();

                    if (lineArray != null && lineArray.Count == 7)
                    {
                        LocationLookup locationLookup = new Umbraco.Extensions.Models.LocationLookup();

                        locationLookup.City = lineArray[2];
                        locationLookup.StateCode = lineArray[3];
                        locationLookup.Latitude = Convert.ToDouble(lineArray[5]);
                        locationLookup.Longitude = Convert.ToDouble(lineArray[6]);
                        StateLookup findState = stateLookupList.Where(p => p.Abbreviation == locationLookup.StateCode).FirstOrDefault();

                        if (findState != null)
                        {
                            locationLookup.State = findState.Name;
                        }
                        else
                        {
                            if (locationLookup.StateCode == "DC")
                            {
                                locationLookup.State = "Washington";
                            }
                            else
                            {
                                int regionId = 0;

                                if (int.TryParse(locationLookup.StateCode, out regionId))
                                {
                                    StateLookup findRegion = stateLookupList.Where(p => p.RegionCode == regionId).FirstOrDefault();

                                    if (findRegion != null)
                                    {
                                        locationLookup.StateCode = findRegion.Abbreviation;
                                        locationLookup.State = findRegion.Name;
                                    }
                                    else
                                    {
                                        throw new Exception("BAD STATE");
                                    }
                                }
                            }
                        }

                        if (false == string.IsNullOrEmpty(locationLookup.State))
                        {
                            using (var db2 = new Database("umbracoDbDSN"))
                            {
                                db2.Insert(locationLookup);
                            }
                        }
                    }
                }

                file.Close();



            }
            catch (Exception ex)
            {
                CacheMessage.Message = DebugStr.ToString() + "\r\n\r\n" + ex.ToString();
            }

            return CacheMessage;
        }

        private void SyncArloEvents()
        {
            string endPoint = ConfigurationManager.AppSettings.Get("Caching:Arlo:Events:Url");
            string username = ConfigurationManager.AppSettings.Get("Caching:Arlo:Username");
            string password = ConfigurationManager.AppSettings.Get("Caching:Arlo:Password");
            string publicEndpoint = ConfigurationManager.AppSettings.Get("Caching:Arlo:PublicEvents:Url") + "&top=200&includeTotalCount=true&format=json";
            Dictionary<int, PublicEvent> publicEvents = GetPublicEvents(publicEndpoint);
            List<Event> events = new List<Event>();
            events = GetEvents(username, password, endPoint, events);
            CacheObjects.SaveCourses(events, publicEvents);
        }

        private Dictionary<int, PublicEvent> GetPublicEvents(string endPoint)
        {
            Dictionary<int, PublicEvent> events = new Dictionary<int, PublicEvent>();
            WebClient client = new WebClient();
            int totalEvents = 0;
            int iteratedEvents = 0;
            do
            {
                var response = client.DownloadString(endPoint + "&skip=" + iteratedEvents); 
                if (string.IsNullOrEmpty(response))
                    break;
                PublicEvents oObj = JsonConvert.DeserializeObject<PublicEvents>(response);
                if (oObj == null)
                    break;
                totalEvents = oObj.TotalCount;
                var items = oObj.Items.ToDictionary(x => x.EventID, x => x);
                events = events.Union(items).ToDictionary(x => x.Key, x => x.Value);
                iteratedEvents += oObj.Count;
            } while (iteratedEvents < totalEvents);
            return events;
        }

        private List<Event> GetEvents(string username, string password, string endpoint, List<Event> events)
        {
            ArloAPI api = new ArloAPI(endpoint, username, password);
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
                    events = GetEvents(username, password, ((IDictionary<String, object>)linkObj)["@href"].ToString(), events);
                    break;
                }
                if (!((IDictionary<String, object>)linkObj).ContainsKey("Event"))
                    continue;
                Event oObj = ((ExpandoObject)linkObj.Event).ToObject<Event>();
                if (oObj == null)
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
                            oObj.Template = GetEventTemplate(username, password, href);
                            break;
                        case "CustomFields":
                            oObj.CustomFields = GetCustomFields(username, password, href);
                            break;
                        case "AdvertisedRegions":
                            oObj.Region = GetEventRegion(username, password, href);
                            break;
                        case "Sessions":
                            oObj.Sessions = GetEventSessions(username, password, href);
                            break;
                        default:
                            break;
                    }
                }
                events.Add(oObj);
            }
            return events;
        }

        private List<Session> GetEventSessions(string username, string password, string href)
        {
            ArloAPI api = new ArloAPI(href, username, password);
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
                    ArloAPI sessionApi = new ArloAPI(dict["@href"].ToString(), username, password);
                    dynamic sessionObj = sessionApi.GetArloResponse();
                    if (sessionObj == null || !((IDictionary<String, object>)sessionObj).ContainsKey("EventSession"))
                        continue;
                    sessions.Add(GetEventSession(username, password, sessionObj));
                }
            }
            return sessions;
        }

        private Session GetEventSession(string username, string password,dynamic oObj)
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
                        session.Venue = GetSessionVenue(username, password, href);
                        break;
                    case "CustomFields":
                        session.CustomFields = GetCustomFields(username, password, href);
                        break;
                    case "Presenters":
                        session.Presenter = GetSessionPresenter(username, password, href);
                        break;
                    default:
                        break;
                }
            }
            return session;
        }

        private ArloContact GetSessionPresenter(string username, string password, string href)
        {
            ArloAPI api = new ArloAPI(href, username, password);
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
                    ArloAPI contactApi = new ArloAPI(linkHref, username, password);
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

        private Venue GetSessionVenue(string username, string password, string href)
        {
            ArloAPI api = new ArloAPI(href, username, password);
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
                    ArloAPI venueApi = new ArloAPI(dict["@href"].ToString(), username, password);
                    dynamic venueObj = venueApi.GetArloResponse();
                    if (venueObj == null || !((IDictionary<String, object>)venueObj).ContainsKey("Venue"))
                        continue;
                    venue = ((ExpandoObject)venueObj.Venue).ToObject<Venue>();
                    foreach (dynamic venueLink in venue.Link)
                    {
                        IDictionary<String, object> venueDict = (IDictionary<String, object>)venueLink;
                        if (!venueDict.ContainsKey("@title"))
                            continue;
                        ArloAPI addressApi = new ArloAPI(venueDict["@href"].ToString(), username, password);
                        switch(venueDict["@title"].ToString()) 
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
                                venue.Description = Convert.ToString(facilityObj.VenueFacilityInformation.Directions);
                                break;
                        }
                    }
                    break;
                }
            }
            return venue;
        }

        private Region GetEventRegion(string username, string password, string href)
        {
            ArloAPI api = new ArloAPI(href, username, password);
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
                    ArloAPI regionApi = new ArloAPI(dict["@href"].ToString(), username, password);
                    dynamic regionObj = regionApi.GetArloResponse();
                    if (regionObj == null || !((IDictionary<String, object>)regionObj).ContainsKey("Region"))
                        continue;
                    region = ((ExpandoObject)regionObj.Region).ToObject<Region>();
                    break;
                }
            }
            return region;
        }

        private EventTemplate GetEventTemplate(string username, string password, string href)
        {
            ArloAPI api = new ArloAPI(href, username, password);
            dynamic oObj = api.GetArloResponse();
            if (oObj == null || !((IDictionary<String, object>)oObj).ContainsKey("EventTemplate"))
                return null;
            return ((ExpandoObject)oObj.EventTemplate).ToObject<EventTemplate>();
        }

        private Dictionary<string, string> GetCustomFields(string username, string password, string href)
        {
            Dictionary<string, string> customFields = new Dictionary<string, string>();
            ArloAPI api = new ArloAPI(href, username, password);
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
                IDictionary<String,object> fieldDict = (IDictionary<String,object>) field;
                if (!fieldDict.ContainsKey("Name") || !fieldDict.ContainsKey("Value"))
                    continue;
                customFields.Add(field.Name.ToString(), field.Value.String);
            }
            return customFields;
        }

        private void SaveCourse(Event oEvent)
        {

        }

        private void ProcessCourses()
        {
            var db = new Database("umbracoDbDSN");

            List<CourseDetail> courseDetailListCurrent = null;

            // Get Cache from DB
            DebugApp("GETTING CURRENT COURSES...", ref DebugStr);
            courseDetailListCurrent = db.Query<CourseDetail>("SELECT * FROM CacheCourseDetail").ToList();
            DebugApp("CURRENT COURSES - Done: " + (courseDetailListCurrent != null ? courseDetailListCurrent.Count.ToString() : "null"), ref DebugStr);

            DebugApp("", ref DebugStr);

            // Get Updated Records
            DebugApp("GETTING UPDATED COURSES...", ref DebugStr);
            List<CourseDetail> courseDetailList = GetCourseDetailList();
            DebugApp("UPDATED COURSES - Done: " + (courseDetailList != null ? courseDetailList.Count.ToString() : "null"), ref DebugStr);

            DebugApp("", ref DebugStr);
            DebugApp("", ref DebugStr);

            DebugApp("PROCESSING COURSES...", ref DebugStr);

            if (courseDetailList != null && courseDetailList.Count > 0)
            {
                foreach (CourseDetail course in courseDetailList)
                {
                    DebugApp(" - Course Id: " + course.Id, ref DebugStr);

                    CourseDetail checkCurrentCourse = courseDetailListCurrent.Where(p => p.Id == course.Id).FirstOrDefault();

                    if (checkCurrentCourse != null)
                    {
                        DebugApp(" - Updated Course: " + course.Id, ref DebugStr);

                        using (var db2 = new Database("umbracoDbDSN"))
                        {
                            db2.Update(course);

                            courseDetailListCurrent.Remove(checkCurrentCourse);
                        }
                    }
                    else
                    {
                        DebugApp(" - Adding Course: " + course.Id, ref DebugStr);

                        using (var db2 = new Database("umbracoDbDSN"))
                        {
                            db2.Insert(course);
                        }
                    }
                }
            }

            DebugApp("DELETING OLD COURSES... ", ref DebugStr);
            if (courseDetailListCurrent != null && courseDetailListCurrent.Count > 0)
            {
                foreach (CourseDetail oldCourse in courseDetailListCurrent)
                {
                    DebugApp(" - Delete Old Course: " + oldCourse.Id, ref DebugStr);
                    using (var db2 = new Database("umbracoDbDSN"))
                    {
                        db2.Delete(oldCourse);
                    }
                }
            }

            DebugApp("DELETING OLD COURSES - DONE", ref DebugStr);
        }


        private void ProcessSeminars()
        {
            var db = new Database("umbracoDbDSN");
            Dictionary<int, int> courseRelations = CacheObjects.GetCourseRelations().ToDictionary(x => x.SimulcastID.Value, x => x.OpenID.Value);
            List<LocationScheduleDetail> locationScheduleDetailListCurrent = null;
            DebugApp("GETTING CURRENT SEMINARS...", ref DebugStr);
            locationScheduleDetailListCurrent = db.Query<LocationScheduleDetail>("SELECT * FROM CacheLocationScheduleDetail").ToList();
            DebugApp("CURRENT SEMINARS - Done: " + (locationScheduleDetailListCurrent != null ? locationScheduleDetailListCurrent.Count.ToString() : "null"), ref DebugStr);

            DebugApp("", ref DebugStr);

            DebugApp("GETTING UPDATED SEMINARS...", ref DebugStr);
            List<LocationScheduleDetail> locationScheduleDetailList = GetLocationScheduleDetailList(courseRelations);
            DebugApp("UPDATED SEMINARS - Done: " + (locationScheduleDetailList != null ? locationScheduleDetailList.Count.ToString() : "null"), ref DebugStr);

            DebugApp("", ref DebugStr);
            DebugApp("", ref DebugStr);

            DebugApp("PROCESSING COURSES...", ref DebugStr);

            if (locationScheduleDetailList != null && locationScheduleDetailList.Count > 0)
            {
                foreach (LocationScheduleDetail seminar in locationScheduleDetailList)
                {
                    DebugApp(" - Seminar Id: " + seminar.Id, ref DebugStr);
                    LocationScheduleDetail checkCurrentSeminar = locationScheduleDetailListCurrent.Where(p => p.Id == seminar.Id).FirstOrDefault();
                    if (checkCurrentSeminar != null)
                    {
                        DebugApp(" - Updated Seminar: " + seminar.Id, ref DebugStr);
                        if (seminar.ScheduleType.ToLower() == "simulcast" && courseRelations.ContainsKey(seminar.CourseId))
                            seminar.CourseId = courseRelations[seminar.CourseId];
                        using (var db2 = new Database("umbracoDbDSN"))
                        {
                            db2.Update(seminar);
                            locationScheduleDetailListCurrent.Remove(checkCurrentSeminar);
                        }
                    }
                    else
                    {
                        DebugApp(" - Adding Seminar: " + seminar.Id, ref DebugStr);

                        using (var db2 = new Database("umbracoDbDSN"))
                        {
                            db2.Insert(seminar);
                        }
                    }
                }
            }

            DebugApp("DELETING OLD SEMINARS... ", ref DebugStr);
            if (locationScheduleDetailListCurrent != null && locationScheduleDetailListCurrent.Count > 0)
            {
                foreach (LocationScheduleDetail oldSeminar in locationScheduleDetailListCurrent)
                {
                    DebugApp(" - Delete Old Seminar: " + oldSeminar.Id, ref DebugStr);
                    using (var db2 = new Database("umbracoDbDSN"))
                    {
                        db2.Delete(oldSeminar);
                    }
                }
            }

            DebugApp("DELETING OLD SEMINARS - DONE", ref DebugStr);
        }


        private List<CourseDetail> GetCourseDetailList()
        {
            List<CourseDetail> courseDetailList = null;

            if (courseDetailList == null)
            {
                DebugApp("Course Detail List to Cache...", ref DebugStr);

                List<COURS> courseList = CacheObjects.GetCourseList(true);


                DebugApp("Umbraco Course Detail List...", ref DebugStr);
                List<UmbracoCourseDetail> umbracoCourseDetailList = GetUmbracoCourseDetailList();


                courseList = courseList.Where(p => p.CourseTier > 0).ToList();

                if (courseList != null && courseList.Count > 0)
                {
                    DebugApp(" - Count: " + courseList.Count, ref DebugStr);

                    courseList = courseList.OrderBy(p => p.CourseTier).ThenBy(t => t.CourseTopicID).ToList();

                    courseDetailList = new List<CourseDetail>();

                    foreach (COURS courseItem in courseList)
                    {
                        CourseDetail courseDetail = new CourseDetail();

                        courseDetail.Id = courseItem.CourseID;
                        courseDetail.TopicId = courseItem.CourseTopicID;
                        courseDetail.CourseTier = courseItem.CourseTier ?? 0;
                        courseDetail.Title = courseItem.TitlePlain;
                        courseDetail.SubTitle = courseItem.WebToolTip;

                        UmbracoCourseDetail umbracoCourseDetail = umbracoCourseDetailList.Where(p => p.CourseId == courseItem.CourseID).FirstOrDefault();

                        courseDetail.ImageUrl = "/images/default-seminar.gif";
                        courseDetail.DetailsUrl = "#";
                        courseDetail.Price = Convert.ToDouble(courseItem.CourseFee);

                        if (umbracoCourseDetail != null)
                        {
                            courseDetail.ImageUrl = umbracoCourseDetail.ImageUrl;
                            courseDetail.DetailsUrl = umbracoCourseDetail.DetailsUrl;
                            courseDetail.NodeId = umbracoCourseDetail.NodeId;

                            if (false == string.IsNullOrEmpty(umbracoCourseDetail.SubTitle))
                            {
                                courseDetail.SubTitle = umbracoCourseDetail.SubTitle;
                            }
                        }

                        courseDetailList.Add(courseDetail);
                    }

                    courseDetailList = courseDetailList.Where(p => p.DetailsUrl != "#").ToList();
                }

                DebugApp(" - Course Detail List Updated", ref DebugStr);
                DebugApp("", ref DebugStr);
            }

            return courseDetailList;
        }


        private List<LocationScheduleDetail> GetLocationScheduleDetailList(Dictionary<int,int> courseRelations)
        {
            int inc = 0;
            List<ScheduleCourseInstructor> scheduleCourseInstructorList = CacheObjects.GetScheduleCourseList(true);
            List<LocationScheduleDetail> locationScheduleDetailList = null;
            if (locationScheduleDetailList == null)
            {
                DebugApp("Location Schedule Detail List to Cache...", ref DebugStr);

                List<SCHEDULE> seminarList = CacheObjects.GetScheduleList(true);

                if (seminarList != null && seminarList.Count > 0)
                {
                    DebugApp(" - Count: " + seminarList.Count, ref DebugStr);

                    locationScheduleDetailList = new List<LocationScheduleDetail>();
                    foreach (SCHEDULE legacySchedule in seminarList)
                    {
                        LocationScheduleDetail locationScheduleDetail = new LocationScheduleDetail();

                        ScheduleCourseInstructor scheduleCourse = scheduleCourseInstructorList.Where(p => p.ScheduleID == legacySchedule.ScheduleID).FirstOrDefault();
                        int courseID = scheduleCourse.CourseID;
                        bool bSimulcast = legacySchedule.ScheduleType.ToLower() == "simulcast";
                        bool bOnline = legacySchedule.ScheduleType.ToLower() == "liveonline";
                        if ((bSimulcast || bOnline) && courseRelations.ContainsKey(courseID))
                            courseID = courseRelations[courseID];
                        COURS legacyCourse = CacheObjects.GetCourseList().Where(p => p.CourseID == courseID).FirstOrDefault();
                        State legacyState = CacheObjects.GetStateList().Where(p => p.StateID == legacySchedule.StateID).FirstOrDefault();
                        City legacyCity = CacheObjects.GetCityAllList().Where(p => p.CityID == legacySchedule.CityID).FirstOrDefault();

                        if (true)
                        {
                            if (scheduleCourse != null && legacyCourse != null && legacyState != null && legacyCity != null)
                            {
                                COURS course = CacheObjects.GetCourseList().Where(p => p.CourseID == scheduleCourse.CourseID).FirstOrDefault();

                                if (course != null)
                                {
                                    locationScheduleDetail.Id = legacySchedule.ScheduleID;
                                    locationScheduleDetail.ParentId = legacySchedule.ScheduleParentID ?? 0;
                                    locationScheduleDetail.ScheduleSeminarNumber = legacySchedule.ScheduleSeminarNumber;
                                    locationScheduleDetail.TopicId = legacyCourse.CourseTopicID;
                                    locationScheduleDetail.CourseId = courseID;
                                    locationScheduleDetail.DaysTitle = CacheObjects.GetDaysTitle(course.CourseFormatID);
                                    locationScheduleDetail.DaysDescription = course.CertTitle1 + (false == string.IsNullOrWhiteSpace(course.CertTitle2) ? " - " + course.CertTitle2 : "");
                                    locationScheduleDetail.Date = legacySchedule.ScheduleDateDescription;
                                    locationScheduleDetail.Price = Convert.ToDouble(scheduleCourse.CourseFee);
                                    locationScheduleDetail.Description = course.GoogleDesc ?? course.TitlePlain;

                                    locationScheduleDetail.City = legacyCity.CityName;
                                    locationScheduleDetail.StateCode = legacyState.StateAbbreviation;
                                    locationScheduleDetail.State = legacyState.StateName;
                                    locationScheduleDetail.ScheduleType = legacySchedule.ScheduleType;
                                    locationScheduleDetail.TrainingKey = legacySchedule.TrainingKey;
                                    // get exact location
                                    Location locationDetail = CacheObjects.GetLocationList().Where(p => p.LocationID == legacySchedule.LocationID).FirstOrDefault();
                                    string sLocationDetails = GetUmbracoSummaryText();
                                    if (!bSimulcast && !bOnline && locationDetail != null)
                                        sLocationDetails = CacheObjects.GetLocationDetails(locationDetail, locationScheduleDetail);
                                    else if (bSimulcast)
                                        sLocationDetails = GetUmbracoSummaryText(1);
                                    else if(bOnline)
                                        sLocationDetails = GetUmbracoSummaryText(2);
                                    locationScheduleDetail.LocationDetails = sLocationDetails;
                                    locationScheduleDetail.CoordinatesObj = legacyCity.Coordinates;
                                    locationScheduleDetail.DateFilter = legacySchedule.ScheduleDate;
                                    locationScheduleDetail.DateMonthYear = locationScheduleDetail.DateFilter.ToString("M-yyyy");
                                    locationScheduleDetail.Distance = 0;
                                    locationScheduleDetail.SeminarId = courseID;
                                    locationScheduleDetail.SeminarTitle = legacyCourse.TitlePlain;
                                    locationScheduleDetail.RegisterUri = course.RegisterUri;
                                    locationScheduleDetail.ViewUri = course.ViewUri;
                                }
                                locationScheduleDetailList.Add(locationScheduleDetail);
                            }
                        }

                        inc++;
                        DebugApp(" - Adding... " + inc, ref DebugStr);
                    }
                }

                DebugApp(" - Location Schedule Detail List Updated", ref DebugStr);
                DebugApp("", ref DebugStr);
            }

            return locationScheduleDetailList;
        }


        private List<UmbracoCourseDetail> GetUmbracoCourseDetailList()
        {
            string json = null;
            HttpClient client = new HttpClient();
            string apiDomain = ConfigurationManager.AppSettings.Get("Cache:UmbracoCourseApiDomain");

            HttpResponseMessage response = client.GetAsync(apiDomain + "/api/contents/CourseList").Result;
            response.EnsureSuccessStatusCode();
            json = response.Content.ReadAsStringAsync().Result;

            return JsonConvert.DeserializeObject<List<UmbracoCourseDetail>>(json);
        }


        private string GetUmbracoSummaryText(int iType = 0)
        {
            string output = null;

            HttpClient client = new HttpClient();
            string apiDomain = ConfigurationManager.AppSettings.Get("Cache:UmbracoCourseApiDomain");

            try
            {
                HttpResponseMessage response = client.GetAsync(apiDomain + "/api/contents/SummaryText" + (iType > 0 ? "/" + iType : "")).Result;
                response.EnsureSuccessStatusCode();
                output = response.Content.ReadAsStringAsync().Result;
                output = output.Trim("\"");
            }
            catch (Exception ex)
            {
                output = "Specific location will be provided via email approximately 4 weeks prior to seminar date.";
            }
            return output;
        }


        private void DebugApp(string line, ref StringBuilder debug)
        {
            Debug.WriteLine(line);
            debug.AppendLine(line);
        }

        private void logCacheLog()
        {
            string referrer = HttpContext.Current.Request.Headers["Referer"];
            using (var db = new americantraincoEntities())
            {
                Cache_Log oObj = new Cache_Log();
                oObj.triggered_on = DateTime.Now;
                oObj.triggered_by = referrer;
                db.Cache_Log.Add(oObj);
                db.SaveChanges();
            }
        } 
    }

    public class CacheMessage
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
