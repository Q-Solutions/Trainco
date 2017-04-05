using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using TPCTrainco.Umbraco.Extensions.Helpers;
using TPCTrainco.Umbraco.Extensions.Models;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace TPCTrainco.Umbraco.Extensions.Controllers.v2
{
    public class ContentsController : ApiController
    {
        [HttpGet]
        public object Course(string id = null)
        {
            string output = "";
            UmbracoCourseDetail seminarDetails = new UmbracoCourseDetail();
            IPublishedContent contentResponse = null;

            seminarDetails.ImageUrl = "/assets/images/default-seminar.gif";
            seminarDetails.DetailsUrl = "#";
            try
            {
                UmbracoHelper UmbracoHelperObj = new UmbracoHelper(UmbracoContext.Current);

                if (false == string.IsNullOrWhiteSpace(id))
                {
                    contentResponse = Helpers.Nodes.Instance.SeminarItems.Where(p => p.GetProperty("courseLink").Value != null && p.GetProperty("courseLink").Value.ToString() == id).FirstOrDefault();

                    if (contentResponse != null)
                    {
                        seminarDetails.ImageUrl = "/assets/images/default-seminar.gif";
                        seminarDetails.DetailsUrl = contentResponse.Url;

                        if (contentResponse != null)
                        {
                            seminarDetails.ImageUrl = contentResponse.GetCropUrl("image", "Image");
                        }

                        if (true == contentResponse.HasValue("searchSummaryText"))
                        {
                            seminarDetails.SubTitle = contentResponse.GetPropertyValue<string>("searchSummaryText");
                        }

                        //output = JsonConvert.SerializeObject(seminarDetails);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return seminarDetails;
        }


        [HttpGet]
        public object CourseList()
        {
            string output = "";
            List<UmbracoCourseDetail> seminarCourseList = new List<UmbracoCourseDetail>();

            try
            {
                UmbracoHelper UmbracoHelperObj = new UmbracoHelper(UmbracoContext.Current);

                IEnumerable<IPublishedContent> seminarList = Helpers.Nodes.Instance.SeminarItems;

                seminarList = Helpers.Nodes.SeminarItemList();

                if (seminarList != null && seminarList.Count() > 0)
                {
                    foreach (IPublishedContent seminarItem in seminarList)
                    {
                        UmbracoCourseDetail courseDetail = new UmbracoCourseDetail();

                        courseDetail.CourseId = 0;
                        courseDetail.NodeId = 0;
                        courseDetail.ImageUrl = "/assets/images/default-seminar.gif";
                        courseDetail.DetailsUrl = "#";

                        if (seminarItem != null)
                        {
                            courseDetail.CourseId = seminarItem.GetPropertyValue<int>("courseLink");
                            courseDetail.NodeId = seminarItem.Id;

                            courseDetail.ImageUrl = seminarItem.GetCropUrl("image", "Image");
                            courseDetail.DetailsUrl = seminarItem.Url;
                            if (true == seminarItem.HasValue("searchSummaryText"))
                            {
                                courseDetail.SubTitle = seminarItem.GetPropertyValue<string>("searchSummaryText");
                            }
                        }

                        seminarCourseList.Add(courseDetail);
                    }
                }

                //if (seminarCourseList != null)
                //{
                //    output = JsonConvert.SerializeObject(seminarCourseList);
                //}
            }
            catch (Exception ex)
            {

            }

            return seminarCourseList;
        }


        [HttpGet]
        public object SummaryText(string id = null)
        {
            string output = "";
            try
            {
                output = Nodes.Instance.SeminarSearch.GetPropertyValue<string>((string.IsNullOrEmpty(id) ? "locationMessage" : "simulcastInfoMessage"));
            }
            catch (Exception ex) {}
            return output;
        }
    }


}
