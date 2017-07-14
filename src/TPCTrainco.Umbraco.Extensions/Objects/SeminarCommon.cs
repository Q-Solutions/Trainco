using MoreLinq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.SqlServer;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using TPCTrainco.Umbraco.Extensions.Helpers;
using TPCTrainco.Umbraco.Extensions.Models;
using TPCTrainco.Umbraco.Extensions.Models.SearchRequest;
using TPCTrainco.Umbraco.Extensions.ViewModels.Search;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace TPCTrainco.Umbraco.Extensions.Objects
{
    public class SeminarCommon
    {
        public List<CourseDetail> CourseDetailList = null;
        public List<LocationScheduleDetail> LocationScheduleDetailList = null;
        public CoordinateDetails locationCoordinates { get; set; }

        public int SchedulesPerPage = 10;

        UmbracoHelper umbracoHelper = new UmbracoHelper(UmbracoContext.Current);

        public SeminarCommon()
        {
            CourseDetailList = CacheObjects.GetCourseDetailList();
            LocationScheduleDetailList = CacheObjects.GetLocationScheduleDetailList();
        }


        protected void FilterByClass(ref List<CourseDetail> courseDetailList, ref List<LocationScheduleDetail> locationScheduleDetailList, SeminarsSearchRequest2 request)
        {
            if (request.ClassId > 0)
            {
                courseDetailList = courseDetailList.Where(p => p.Id == request.ClassId).ToList();

                locationScheduleDetailList = locationScheduleDetailList.Where(p => p.CourseId == request.ClassId).ToList();
            }
        }


        protected void FilterByDate(ref List<LocationScheduleDetail> locationScheduleDetailList, SeminarsSearchRequest2 request)
        {
            if (request.DateRage == null || request.DateRage.Min == null | request.DateRage.Max == null)
            {
                request.DateRage = new DateRange();

                request.DateRage.Min = DateTime.Parse(DateTime.Now.ToString("M/1/yyyy"));
                request.DateRage.Max = DateTime.Parse(request.DateRage.Min.AddMonths(4).AddDays(-1).ToString("M/d/yyyy"));
            }

            locationScheduleDetailList = locationScheduleDetailList.Where(p => p.DateFilter >= request.DateRage.Min && p.DateFilter <= request.DateRage.Max).ToList();
        }


        protected void FilterByLocation(ref List<LocationScheduleDetail> locationScheduleDetailList, SeminarsSearchRequest2 request)
        {
            string debug = "";

            if (request.Radius <= 0)
            {
                request.Radius = 50;
            }

            if (request.Location == "undefined" || request.Location == "null")
            {
                request.Location = "";
            }

            debug += "request.Radius: " + request.Radius + "\r\n";
            debug += "request.Location: " + request.Location + "\r\n";

            if (false == string.IsNullOrEmpty(request.Location))
            {
                locationCoordinates = GeoCoordinates.GetCoordinateDetailsFromCityState(request.Location);

                if (locationCoordinates != null)
                {
                    debug += "LocationCoordinates.City: " + locationCoordinates.City + "\r\n";
                    debug += "LocationCoordinates.Latitude: " + locationCoordinates.Latitude + "\r\n";
                    debug += "LocationCoordinates.Longitude: " + locationCoordinates.Longitude + "\r\n";

                    locationScheduleDetailList = FindLocationSeminarCatalog(locationScheduleDetailList, locationCoordinates, request);
                }
                else
                {
                    debug += "LocationCoordinates IS NULL!\r\n";
                    //List<LocationScheduleDetail> locationSearch = CacheObjects.GetLocationScheduleDetailList();
                    //debug += "Trying lookup method...\r\n";
                    if (request.bLocationPage)
                        locationScheduleDetailList.Clear();
                        //locationScheduleDetailList = locationScheduleDetailList.Where(x => x.ScheduleType.ToLower() == "simulcast").ToList();
                    //if (locationSearch != null && locationSearch.Any())
                    //{
                    //    LocationScheduleDetail findLocation = locationSearch.Where(p => request.Location.ToLower().IndexOf(p.City.ToLower()) >= 0).FirstOrDefault();
                    //    if (findLocation != null)
                    //    {
                    //        debug += "FOUND!\r\n";
                    //        locationCoordinates = new CoordinateDetails();
                    //        locationCoordinates.City = findLocation.City;
                    //        locationCoordinates.DbGeography = findLocation.CoordinatesObj;
                    //        locationCoordinates.Latitude = locationCoordinates.DbGeography.Latitude ?? 0;
                    //        locationCoordinates.Longitude = locationCoordinates.DbGeography.Longitude ?? 0;
                    //        locationCoordinates.State = findLocation.State;
                    //        locationCoordinates.StateCode = findLocation.StateCode;
                    //    }
                    //}
                }
            }

            //SendDebugEmail(debug, "TPCTrainco.com Seminar Search Debug");
        }


        protected void FilterByTopic(ref List<CourseDetail> courseDetailList, ref List<LocationScheduleDetail> locationScheduleDetailList, SeminarsSearchRequest2 request)
        {
            if (request.Topics != null && request.Topics.Length > 0)
            {
                List<int> topicArray = new List<int>();

                foreach (string classTopic in request.Topics)
                {
                    if (false == string.IsNullOrWhiteSpace(classTopic) && classTopic != "undefined" && classTopic != "null")
                    {
                        int topicId = Objects.Topics.TopicIdByShortName(classTopic);

                        if (topicId > int.MinValue)
                        {
                            topicArray.Add(topicId);
                        }
                    }
                }

                if (topicArray.Count > 0)
                {
                    courseDetailList = courseDetailList.Where(p => topicArray.Any(r => r == p.TopicId)).ToList();
                    locationScheduleDetailList = locationScheduleDetailList.Where(p => topicArray.Any(r => r == p.TopicId)).ToList();
                }
            }
        }


        protected void FilterByKeyword(ref List<LocationScheduleDetail> locationScheduleDetailList, SeminarsSearchRequest2 request)
        {
            if (false == string.IsNullOrWhiteSpace(request.Keywords) && request.Keywords != "undefined" && request.Keywords != "null")
            {
                string keywordsSearch = request.Keywords.ToLower();

                locationScheduleDetailList = locationScheduleDetailList.Where(p => p.Description.ToLower().Contains(keywordsSearch) ||
                    p.DaysDescription.ToLower().Contains(keywordsSearch) ||
                    p.LocationDetails.ToLower().Contains(keywordsSearch) ||
                    p.State.ToLower().Contains(keywordsSearch) ||
                    p.City.ToLower().Contains(keywordsSearch)).ToList();
            }
        }


        private List<LocationScheduleDetail> FindLocationSeminarCatalog(List<LocationScheduleDetail> locationScheduleDetailSearch, CoordinateDetails coordinateDetails, SeminarsSearchRequest2 request)
        {
            List<LocationScheduleDetail> resultSearch = null;
            double radiusSearch = request.Radius;

            while ((resultSearch == null || resultSearch.Count <= 0) && radiusSearch < 4000)
            {
                resultSearch = RadialSearchSeminarCatalog(locationScheduleDetailSearch, coordinateDetails, radiusSearch);

                radiusSearch *= 2;
            }

            return resultSearch;
        }


        /// <summary>
        /// Perform a radial search in miles on seminar catalog
        /// </summary>
        /// <param name="seminarListSearch"></param>
        /// <param name="coordinateDetails"></param>
        /// <param name="radiusInMiles"></param>
        /// <returns></returns>
        private List<LocationScheduleDetail> RadialSearchSeminarCatalog(List<LocationScheduleDetail> locationScheduleDetailSearch, CoordinateDetails coordinateDetails, double radiusInMiles)
        {
            List<LocationScheduleDetail> tempSearch = null;

            tempSearch = locationScheduleDetailSearch.Where(p => p.Coordinates == null).ToList();

            if (tempSearch != null && tempSearch.Count > 0)
            {
                Debug.WriteLine("Updating coordinates for locations...");

                GeoCoordinates.UpdateCityCoordinates();
            }

            tempSearch = locationScheduleDetailSearch.Where(x => x.ScheduleType.ToLower() == "simulcast" || (x.CoordinatesObj != null && x.CoordinatesObj.Distance(coordinateDetails.DbGeography) * 0.00062 <= radiusInMiles))
                    .OrderBy(p => p.CoordinatesObj.Distance(coordinateDetails.DbGeography)).ToList();

            foreach (LocationScheduleDetail updateCoordinates in tempSearch)
            {
                updateCoordinates.Distance = updateCoordinates.CoordinatesObj.Distance(coordinateDetails.DbGeography);
            }

            return tempSearch;
        }


        private void SendDebugEmail(string emailBody, string subject = "")
        {
            List<string> emailToList = null;

            if (ConfigurationManager.AppSettings["LogToEmail:CCError"] != null && ConfigurationManager.AppSettings.Get("LogToEmail:CCError").Length > 0)
            {
                emailToList = new List<string>();

                emailToList = ConfigurationManager.AppSettings.Get("LogToEmail:CCError").Split(';').ToList();

                Helpers.Email email = new Email();

                email.EmailFrom = "website@tpctrainco.com";
                email.EmailToList = emailToList;
                if (false == string.IsNullOrEmpty(subject))
                {
                    email.Subject = subject;
                }
                else
                {
                    email.Subject = "TPCTrainco.com Registration Error";
                }
                email.IsBodyHtml = false;

                string body = emailBody;

                body += "\r\n\r\n";
                body += HttpContext.Current.Request.Url.AbsoluteUri;
                body += "\r\n\r\n";
                body += HttpContext.Current.Request.UserHostAddress;
                body += "\r\n\r\n";
                body += HttpContext.Current.Request.UserAgent;
                body += "\r\n\r\n";

                email.Body = body;

                email.SendEmail();
            }
        }
    }
}
