﻿using System;
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

namespace TPCTrainco.Cache.Controllers
{
    [RoutePrefix("api/cache")]
    public class CacheController : ApiController
    {
        public StringBuilder DebugStr = new StringBuilder();
        public CacheMessage CacheMessage = new CacheMessage();
        

        // GET api/cache/update
        [HttpGet]
        public CacheMessage Index()
        {
            DebugApp("-= CACHE PROCESS START =-", ref DebugStr);
            DebugApp("", ref DebugStr);

            CacheMessage.Success = false;

            try
            {
                ProcessCourses();

                ProcessSeminars();

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

            List<LocationScheduleDetail> locationScheduleDetailListCurrent = null;

            DebugApp("GETTING CURRENT SEMINARS...", ref DebugStr);
            locationScheduleDetailListCurrent = db.Query<LocationScheduleDetail>("SELECT * FROM CacheLocationScheduleDetail").ToList();
            DebugApp("CURRENT SEMINARS - Done: " + (locationScheduleDetailListCurrent != null ? locationScheduleDetailListCurrent.Count.ToString() : "null"), ref DebugStr);

            DebugApp("", ref DebugStr);

            DebugApp("GETTING UPDATED SEMINARS...", ref DebugStr);
            List<LocationScheduleDetail> locationScheduleDetailList = GetLocationScheduleDetailList();
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

                List<COURS> courseList = CacheObjects.GetCourseList();


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


        private List<LocationScheduleDetail> GetLocationScheduleDetailList()
        {
            int inc = 0;

            string defaultSearchLocationText = GetUmbracoSummaryText();

            List<LocationScheduleDetail> locationScheduleDetailList = null;

            if (locationScheduleDetailList == null)
            {
                DebugApp("Location Schedule Detail List to Cache...", ref DebugStr);

                List<SCHEDULE> seminarList = CacheObjects.GetScheduleList();

                if (seminarList != null && seminarList.Count > 0)
                {
                    DebugApp(" - Count: " + seminarList.Count, ref DebugStr);

                    locationScheduleDetailList = new List<LocationScheduleDetail>();

                    foreach (SCHEDULE legacySchedule in seminarList)
                    {
                        LocationScheduleDetail locationScheduleDetail = new LocationScheduleDetail();

                        ScheduleCourseInstructor scheduleCourse = CacheObjects.GetScheduleCourseList().Where(p => p.ScheduleID == legacySchedule.ScheduleID).FirstOrDefault();
                        COURS legacyCourse = CacheObjects.GetCourseList().Where(p => p.CourseID == scheduleCourse.CourseID).FirstOrDefault();
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
                                    locationScheduleDetail.CourseId = scheduleCourse.CourseID;
                                    locationScheduleDetail.DaysTitle = CacheObjects.GetDaysTitle(course.CourseFormatID);
                                    locationScheduleDetail.DaysDescription = course.CertTitle1 + (false == string.IsNullOrWhiteSpace(course.CertTitle2) ? " - " + course.CertTitle2 : "");
                                    locationScheduleDetail.Date = legacySchedule.ScheduleDateDescription;
                                    locationScheduleDetail.Price = Convert.ToDouble(course.CourseFee);
                                    locationScheduleDetail.Description = course.GoogleDesc ?? course.TitlePlain;

                                    locationScheduleDetail.City = legacyCity.CityName;
                                    locationScheduleDetail.StateCode = legacyState.StateAbbreviation;
                                    locationScheduleDetail.State = legacyState.StateName;

                                    // get exact location
                                    Location locationDetail = CacheObjects.GetLocationList().Where(p => p.LocationID == legacySchedule.LocationID).FirstOrDefault();

                                    if (locationDetail != null)
                                    {
                                        locationScheduleDetail.LocationDetails = CacheObjects.GetLocationDetails(locationDetail, locationScheduleDetail);
                                    }
                                    else
                                    {
                                        locationScheduleDetail.LocationDetails = defaultSearchLocationText;
                                    }

                                    locationScheduleDetail.CoordinatesObj = legacyCity.Coordinates;
                                    locationScheduleDetail.DateFilter = legacySchedule.ScheduleDate;
                                    locationScheduleDetail.DateMonthYear = locationScheduleDetail.DateFilter.ToString("M-yyyy");
                                    locationScheduleDetail.Distance = 0;

                                    locationScheduleDetail.SeminarId = scheduleCourse.CourseID;
                                    locationScheduleDetail.SeminarTitle = legacyCourse.TitlePlain;
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


        private string GetUmbracoSummaryText()
        {
            string output = null;

            HttpClient client = new HttpClient();
            string apiDomain = ConfigurationManager.AppSettings.Get("Cache:UmbracoCourseApiDomain");

            try
            {
                HttpResponseMessage response = client.GetAsync(apiDomain + "/api/contents/SummaryText").Result;
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
    }

    public class CacheMessage
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
