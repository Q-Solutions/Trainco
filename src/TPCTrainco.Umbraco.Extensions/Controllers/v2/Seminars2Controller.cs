using System;
using System.Web;
using System.Web.Http;
using TPCTrainco.Umbraco.Extensions.Models.SearchRequest;
using TPCTrainco.Umbraco.Extensions.Objects;
using TPCTrainco.Umbraco.Extensions.ViewModels.Search;

namespace TPCTrainco.Umbraco.Extensions.Controllers.v2
{
    public class Seminars2Controller : ApiController
    {
        [HttpGet]
        public object Search(string id = null)
        {
            SearchResponse searchResponse = new SearchResponse();

            searchResponse.Success = true;
            searchResponse.ErrorMessage = "";

            try
            {
                int classId = 0;
                int page = -1;

                SeminarsSearchRequest2 searchRequest = new SeminarsSearchRequest2();

                searchRequest.DateRage = new DateRange();
                searchRequest.DateRage.Min = DateTime.Parse(DateTime.Now.ToString("M/1/yyyy"));
                searchRequest.DateRage.Max = DateTime.Parse(searchRequest.DateRage.Min.AddMonths(4).AddDays(-1).ToString("M/d/yyyy"));
                bool bSimulcast = HttpContext.Current.Request.QueryString["simulcast"] == "1";
                searchRequest.bLocationPage = HttpContext.Current.Request.QueryString["locationPage"] == "1";
                if (false == string.IsNullOrWhiteSpace(HttpContext.Current.Request.QueryString["keyword"]))
                {
                    searchRequest.Keywords = HttpContext.Current.Request.QueryString.Get("keyword");
                }
                if (false == string.IsNullOrWhiteSpace(HttpContext.Current.Request.QueryString["topics"]))
                {
                    searchRequest.Topics = HttpContext.Current.Request.QueryString.Get("topics").Split(',');
                }
                if (!bSimulcast && !string.IsNullOrWhiteSpace(HttpContext.Current.Request.QueryString["location"]))
                {
                    searchRequest.Location = HttpContext.Current.Request.QueryString.Get("location");

                    if (false == string.IsNullOrEmpty(searchRequest.Location))
                    {
                        if (searchRequest.Location.ToLower() == "all locations")
                        {
                            searchRequest.Location = "";
                        }
                    }
                    
                }
                if (!bSimulcast && !string.IsNullOrWhiteSpace(HttpContext.Current.Request.QueryString["radius"]))
                {
                    double radius = 50;

                    double.TryParse(HttpContext.Current.Request.QueryString.Get("radius"), out radius);

                    searchRequest.Radius = radius;
                }
                if (false == string.IsNullOrWhiteSpace(HttpContext.Current.Request.QueryString["date-start"]))
                {
                    DateTime dateStart = searchRequest.DateRage.Min;
                    DateTime.TryParse(HttpContext.Current.Request.QueryString.Get("date-start"), out dateStart);

                    searchRequest.DateRage.Min = dateStart;
                }
                if (false == string.IsNullOrWhiteSpace(HttpContext.Current.Request.QueryString["date-end"]))
                {
                    DateTime dateEnd = searchRequest.DateRage.Max;
                    DateTime.TryParse(HttpContext.Current.Request.QueryString.Get("date-end"), out dateEnd);

                    searchRequest.DateRage.Max = dateEnd;
                }
                if (false == string.IsNullOrWhiteSpace(HttpContext.Current.Request.QueryString["page"]))
                {
                    int.TryParse(HttpContext.Current.Request.QueryString.Get("page"), out page);
                }
                searchRequest.Page = page;
                searchRequest.Simulcast = bSimulcast;

                if (false == string.IsNullOrWhiteSpace(id))
                {
                    int.TryParse(id, out classId);

                    searchRequest.ClassId = classId;
                }

                SeminarSearch seminarsObj = new SeminarSearch();

                searchResponse.Seminars = seminarsObj.SearchSeminars(searchRequest);

            }
            catch (Exception ex)
            {
                searchResponse.Success = false;
                searchResponse.ErrorMessage = ex.ToString();
            }

            return searchResponse;
        }

        [HttpGet]
        public object List()
        {
            SeminarListResponse listResponse = new SeminarListResponse();

            listResponse.Success = true;
            listResponse.ErrorMessage = "";

            try
            {
                SeminarSearch seminarsObj = new SeminarSearch();

                listResponse.Seminars = seminarsObj.ListSeminars();
            }
            catch (Exception ex)
            {
                listResponse.Success = false;
                listResponse.ErrorMessage = ex.ToString();
            }

            return listResponse;
        }
    }
}
