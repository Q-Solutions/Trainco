using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;
using System.Configuration;
using MoreLinq;
using System.Diagnostics;
using TPCTrainco.Umbraco.Extensions.Models;
using Umbraco.Web;
using Umbraco.Core.Models;
using TPCTrainco.Umbraco.Extensions.Helpers;
using System.Text.RegularExpressions;
using Umbraco.Core.Persistence;

namespace TPCTrainco.Umbraco.Extensions.Objects
{
    public class CacheObjects
    {
        public static DateTime dateStart = DateTime.Now.AddDays(-7);
        public static DateTime dateEnd = DateTime.Now.AddMonths(18);

        public static List<Seminar_Catalog> GetSeminarList(bool forceCacheRefresh = false)
        {
            string cacheKey = "SeminarList";
            int cacheUpdateInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Caching:Minutes:SeminarList"));
            ObjectCache cache = MemoryCache.Default;

            List<Seminar_Catalog> seminarList = cache.Get(cacheKey) as List<Seminar_Catalog>;

            if (true == forceCacheRefresh || seminarList == null)
            {
                Debug.WriteLine("Adding Seminar List to Cache...");

                using (var db = new americantraincoEntities())
                {
                    seminarList = db.Seminar_Catalog.Where(p => p.WeekOf >= dateStart && p.WeekOf < dateEnd && p.CourseTier > 0).ToList();
                }

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
                cache.Add(cacheKey, seminarList, policy);

                Debug.WriteLine(" - Seminar List Cache Updated");
                Debug.WriteLine("");
            }

            return seminarList;
        }


        public static List<SCHEDULE> GetScheduleList(bool forceCacheRefresh = false)
        {
            string cacheKey = "ScheduleList";
            int cacheUpdateInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Caching:Minutes:ScheduleList"));
            ObjectCache cache = MemoryCache.Default;

            List<SCHEDULE> scheduleList = cache.Get(cacheKey) as List<SCHEDULE>;

            if (true == forceCacheRefresh || scheduleList == null)
            {
                Debug.WriteLine("Adding Schedule List to Cache...");

                using (var db = new americantraincoEntities())
                {
                    DateTime dateStart = DateTime.Now.AddDays(-7);
                    DateTime dateEnd = DateTime.Now.AddMonths(18);

                    scheduleList = db.SCHEDULES.Where(p => p.Active == 1 && p.ScheduleDate >= dateStart && p.ScheduleDate < dateEnd).ToList();
                }

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
                cache.Add(cacheKey, scheduleList, policy);

                Debug.WriteLine(" - Schedule List Cache Updated");
                Debug.WriteLine("");
            }

            return scheduleList;
        }

        public static List<SCHEDULE> GetScheduleListAll(bool forceCacheRefresh = false)
        {
            string cacheKey = "ScheduleList";
            int cacheUpdateInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Caching:Minutes:ScheduleListAll"));
            ObjectCache cache = MemoryCache.Default;

            List<SCHEDULE> scheduleList = cache.Get(cacheKey) as List<SCHEDULE>;

            if (true == forceCacheRefresh || scheduleList == null)
            {
                Debug.WriteLine("Adding All Schedule List to Cache...");

                using (var db = new americantraincoEntities())
                {
                    DateTime dateStart = DateTime.Now.AddDays(-7);
                    DateTime dateEnd = DateTime.Now.AddMonths(18);

                    scheduleList = db.SCHEDULES.ToList();
                }

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
                cache.Add(cacheKey, scheduleList, policy);

                Debug.WriteLine(" - All Schedule List Cache Updated");
                Debug.WriteLine("");
            }

            return scheduleList;
        }


        public static List<COURS> GetCourseList(bool forceCacheRefresh = false)
        {
            string cacheKey = "CourseList";
            int cacheUpdateInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Caching:Minutes:Courses"));
            ObjectCache cache = MemoryCache.Default;

            List<COURS> courseList = cache.Get(cacheKey) as List<COURS>;

            if (true == forceCacheRefresh || courseList == null)
            {
                Debug.WriteLine("Adding Course List to Cache");

                using (var db = new americantraincoEntities())
                {
                    courseList = db.COURSES.Where(p => p.Active == 1).ToList();
                }

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
                cache.Add(cacheKey, courseList, policy);

                Debug.WriteLine(" - Course List Cache Updated");
                Debug.WriteLine("");
            }

            return courseList;
        }


        public static List<Location> GetLocationList()
        {
            string cacheKey = "LocationList";
            int cacheUpdateInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Caching:Minutes:Locations"));
            ObjectCache cache = MemoryCache.Default;

            List<Location> locationList = cache.Get(cacheKey) as List<Location>;

            if (locationList == null)
            {
                Debug.WriteLine("Adding Location List to Cache...");

                using (var db = new americantraincoEntities())
                {
                    locationList = db.Locations.Where(p => p.Active == 1 && p.LocationID != 1 && p.LocationID != 3).ToList();
                }

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
                cache.Add(cacheKey, locationList, policy);

                Debug.WriteLine(" - Location List Cache Updated");
                Debug.WriteLine("");
            }

            return locationList;
        }


        public static List<ScheduleCourseInstructor> GetScheduleCourseList(bool forceCacheRefresh = false)
        {
            string cacheKey = "ScheduleCourseList";
            int cacheUpdateInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Caching:Minutes:ScheduleCourses"));
            ObjectCache cache = MemoryCache.Default;

            List<ScheduleCourseInstructor> scheduleCourseList = cache.Get(cacheKey) as List<ScheduleCourseInstructor>;

            if (true == forceCacheRefresh || scheduleCourseList == null)
            {
                Debug.WriteLine("Adding ScheduleCourse List to Cache...");

                using (var db = new americantraincoEntities())
                {
                    scheduleCourseList = db.Set<ScheduleCourseInstructor>().ToList();
                }

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
                cache.Add(cacheKey, scheduleCourseList, policy);

                Debug.WriteLine(" - ScheduleCourse List Cache Updated");
                Debug.WriteLine("");
            }

            return scheduleCourseList;
        }


        public static List<CourseFormat> GetCourseFormatList()
        {
            string cacheKey = "CourseFormatList";
            int cacheUpdateInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Caching:Minutes:CourseFormats"));
            ObjectCache cache = MemoryCache.Default;

            List<CourseFormat> courseFormatList = cache.Get(cacheKey) as List<CourseFormat>;

            if (courseFormatList == null)
            {
                Debug.WriteLine("Adding CourseFormat List to Cache...");

                using (var db = new americantraincoEntities())
                {
                    courseFormatList = db.CourseFormats.Where(p => p.Active == 1).ToList();
                }

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
                cache.Add(cacheKey, courseFormatList, policy);

                Debug.WriteLine(" - CourseFormat List Cache Updated");
                Debug.WriteLine("");
            }

            return courseFormatList;
        }

        public static List<CourseTopic> GetCourseTopicList()
        {
            string cacheKey = "CourseTopicList";
            int cacheUpdateInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Caching:Minutes:CourseTopics"));
            ObjectCache cache = MemoryCache.Default;

            List<CourseTopic> courseTopicList = cache.Get(cacheKey) as List<CourseTopic>;

            if (courseTopicList == null)
            {
                Debug.WriteLine("Adding CourseTopic List to Cache...");

                using (var db = new americantraincoEntities())
                {
                    courseTopicList = db.CourseTopics.Where(p => p.Active == 1 && p.CourseTopicName.Substring(0, 1) != "x" &&
                        p.CourseTopicName.Substring(0, 1) != "y" &&
                        p.CourseTopicName.Substring(0, 1) != "z").ToList();
                }

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
                cache.Add(cacheKey, courseTopicList, policy);

                Debug.WriteLine(" - CourseTopic List Cache Updated");
                Debug.WriteLine("");
            }

            return courseTopicList;
        }


        public static List<City> GetCityList()
        {
            string cacheKey = "CityList";
            int cacheUpdateInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Caching:Minutes:CityList"));
            ObjectCache cache = MemoryCache.Default;

            List<City> cityList = cache.Get(cacheKey) as List<City>;

            if (cityList == null)
            {
                Debug.WriteLine("Adding City List to Cache...");

                using (var db = new americantraincoEntities())
                {
                    cityList = db.Cities.DistinctBy(p => p.MailCode).Where(p => p.Active == 1).ToList();
                }

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
                cache.Add(cacheKey, cityList, policy);

                Debug.WriteLine(" - City List Cache Updated");
                Debug.WriteLine("");
            }

            return cityList;
        }


        public static List<City> GetCityAllList()
        {
            string cacheKey = "CityAllList";
            int cacheUpdateInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Caching:Minutes:CityList"));
            ObjectCache cache = MemoryCache.Default;

            List<City> cityList = cache.Get(cacheKey) as List<City>;

            if (cityList == null)
            {
                Debug.WriteLine("Adding All Cities List to Cache...");

                using (var db = new americantraincoEntities())
                {
                    cityList = db.Cities.Where(p => p.Active == 1).ToList();
                }

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
                cache.Add(cacheKey, cityList, policy);

                Debug.WriteLine(" - All Cities List Cache Updated");
                Debug.WriteLine("");
            }

            return cityList;
        }


        public static List<State> GetStateList()
        {
            string cacheKey = "StateList";
            int cacheUpdateInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Caching:Minutes:StateList"));
            ObjectCache cache = MemoryCache.Default;

            List<State> stateList = cache.Get(cacheKey) as List<State>;

            if (stateList == null)
            {
                Debug.WriteLine("Adding State List to Cache...");

                using (var db = new americantraincoEntities())
                {
                    stateList = db.States.Where(p => p.Active == 1 && p.RepRegion > 0 && p.Sort > 0).OrderBy(o => o.Sort).ToList();
                }

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
                cache.Add(cacheKey, stateList, policy);

                Debug.WriteLine(" - State List Cache Updated");
            }

            return stateList;
        }


        public static List<Country> GetCountryList()
        {
            string cacheKey = "CountryList";
            int cacheUpdateInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Caching:Minutes:CountryList"));
            ObjectCache cache = MemoryCache.Default;

            List<Country> countryList = cache.Get(cacheKey) as List<Country>;

            if (countryList == null)
            {
                Debug.WriteLine("Adding Country List to Cache...");

                using (var db = new americantraincoEntities())
                {
                    countryList = db.Countries.OrderBy(o => o.SortOrder).ToList();
                }

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
                cache.Add(cacheKey, countryList, policy);

                Debug.WriteLine(" - Country List Cache Updated");
                Debug.WriteLine("");
            }

            return countryList;
        }




        public static List<CourseDetail> GetCourseDetailList(bool forceCacheRefresh = false)
        {
            string cacheKey = "CourseDetailList";
            int cacheUpdateInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Caching:Minutes:CourseDetailList"));
            ObjectCache cache = MemoryCache.Default;

            List<CourseDetail> courseDetailList = cache.Get(cacheKey) as List<CourseDetail>;

            if (true == forceCacheRefresh || courseDetailList == null)
            {
                Debug.WriteLine("Course Detail List to Cache...");

                using (var db = new Database("umbracoDbDSN"))
                {
                    courseDetailList = db.Query<CourseDetail>("SELECT * FROM CacheCourseDetail").ToList();
                }

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
                cache.Add(cacheKey, courseDetailList, policy);

                Debug.WriteLine(" - Course Detail List Cache Updated");
                Debug.WriteLine("");
            }

            return courseDetailList;
        }


        public static List<LocationScheduleDetail> GetLocationScheduleDetailList(bool forceCacheRefresh = false)
        {
            string cacheKey = "LocationScheduleDetailList";
            int cacheUpdateInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Caching:Minutes:LocationScheduleDetailList"));
            ObjectCache cache = MemoryCache.Default;
            int inc = 0;

            List<LocationScheduleDetail> locationScheduleDetailList = cache.Get(cacheKey) as List<LocationScheduleDetail>;

            if (true == forceCacheRefresh || locationScheduleDetailList == null)
            {
                Debug.WriteLine("Location Schedule Detail List to Cache...");

                using (var db = new Database("umbracoDbDSN"))
                {
                    locationScheduleDetailList = db.Query<LocationScheduleDetail>("SELECT * FROM CacheLocationScheduleDetail").ToList();
                }

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
                cache.Add(cacheKey, locationScheduleDetailList, policy);

                Debug.WriteLine(" - Location Schedule Detail List Cache Updated");
                Debug.WriteLine("");
            }

            return locationScheduleDetailList;
        }


        /// <summary>
        /// Get the schedule's days title (Day 1, Day 2, Both Days, etc)
        /// </summary>
        /// <param name="courseFormatId"></param>
        /// <returns></returns>
        public static string GetDaysTitle(int courseFormatId)
        {
            string daysTitle = "";

            CourseFormat courseFormat = GetCourseFormatList().Where(p => p.CourseFormatID == courseFormatId).FirstOrDefault();

            if (courseFormat != null)
            {
                daysTitle = courseFormat.CourseFormatName;
            }

            return daysTitle;
        }


        public static string GetLocationDetails(Location locationDetail, LocationScheduleDetail locationScheduleDetail)
        {
            string output = "";

            if (locationDetail.LocationNotes != null && locationDetail.LocationNotes.IndexOf("<b>LOCATION</b>") >= 0)
            {
                string locationDetails = locationDetail.LocationNotes;

                if (false == string.IsNullOrWhiteSpace(locationDetails))
                {
                    string location = "";

                    List<string> locationArray = locationDetails.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

                    if (locationArray == null || locationArray.Count <= 1)
                    {
                        locationArray = locationDetails.Split(new string[] { "\r" }, StringSplitOptions.None).ToList();
                    }

                    if (locationArray != null && locationArray.Count > 0)
                    {
                        //location = Regex.Match(locationDetails, String.Format(@"{0}\s(?<words>[\w\s]+)\s{1}", leftWord, rightWord)).Groups["words"].Value;

                        bool foundLocation = false;

                        foreach (string line in locationArray)
                        {
                            if (line == "")
                            {
                                foundLocation = false;
                            }

                            if (true == foundLocation)
                            {
                                location += line + ", " + Environment.NewLine;
                            }


                            if (line.IndexOf("LOCATION") >= 0)
                            {
                                foundLocation = true;
                            }
                        }
                    }

                    location = location.TrimEnd(new char[] { '\r', '\n', ' ', ',' });
                    location = location.TrimEnd(' ');
                    location = location.TrimEnd(',');
                    location = location.Trim();
                    output = location;

                    //var startTag = "<b>LOCATION";
                    //int startIndex = locationDetails.IndexOf(startTag) + startTag.Length;
                    //int endIndex = locationDetails.IndexOf("<b>", startIndex);
                    //output = locationDetails.Substring(startIndex, endIndex - startIndex);

                    //output = output.Replace("<b>LOCATION", "");
                    //output = Regex.Replace(output, "<b>", "");
                    //output = Regex.Replace(output, "</b>", "");
                    //output = output.Trim();
                    //output = output.Replace("\r", "<br />" + Environment.NewLine);
                }
            }
            if (true == string.IsNullOrWhiteSpace(output))
            {
                output = locationDetail.LocationName + ", " + locationScheduleDetail.City + ", " + locationScheduleDetail.StateCode + " (Street address and directions will be provided via email.)";
            }


            //<b>CNTOROTH3</b>

            //<b>LOCATION</b>
            //Toronto Airport West Hotel
            //5444 Dixie Rd
            //Mississauga, ON L4W 2L2

            //<b>PHONE</b>
            //905-624-1144

            //<b>DIRECTIONS</b>
            //From Airport: Take 427 South to 401 West, exit Dixie Road South and proceed two lights South on Dixie Road and turn right on Aerowood Drive and left into the Hotel driveway.

            //<b>HOTEL INFORMATION</b>
            //Please feel free to contact the hotel directly to make room reservations and to inquire of any discounts that may apply for American Trainco attendees. 


            return output;
        }

        public static List<CourseRelation> GetCourseRelations()
        {
            List<CourseRelation> courseRelations = new List<CourseRelation>();
            using (var db = new americantraincoEntities())
            {
                courseRelations = db.CourseRelations.Where(p => p.OpenID.HasValue && p.SimulcastID.HasValue).ToList();
            }
            return courseRelations;
        }


        public static List<SeminarCitiesActive> GetAcitveCitiesList()
        {
            string cacheKey = "ActiveCitiesList";
            int cacheUpdateInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Caching:Minutes:ActiveCitiesList"));
            ObjectCache cache = MemoryCache.Default;
            List<SeminarCitiesActive> cityList = cache.Get(cacheKey) as List<SeminarCitiesActive>;
            if (cityList != null)
                return cityList;
            Debug.WriteLine("Adding Active Cities List to Cache...");
            using (var db = new americantraincoEntities())
            {
                cityList = db.SeminarCitiesActives.DistinctBy(x => x.CityName).ToList();
            }
            CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheUpdateInMinutes) };
            cache.Add(cacheKey, cityList, policy);
            Debug.WriteLine(" - Active Cities List Cache Updated");
            Debug.WriteLine("");
            return cityList;
        }
    }
}