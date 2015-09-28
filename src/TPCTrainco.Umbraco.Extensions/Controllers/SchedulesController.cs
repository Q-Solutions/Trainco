﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using TPCTrainco.Umbraco.Extensions.Models;
using TPCTrainco.Umbraco.Extensions.Models.SearchRequest;
using TPCTrainco.Umbraco.Extensions.Objects;
using TPCTrainco.Umbraco.Extensions.ViewModels;
using TPCTrainco.Umbraco.Extensions.ViewModels.Backbone;

namespace TPCTrainco.Umbraco.Extensions.Controllers
{
    public class SchedulesController : ApiController
    {
        [HttpPost]
        public List<Sch> SearchByLocation([FromBody] dynamic json)
        {
            List<Sch> resultsList = null;
            Objects.Seminars seminarsObj = new Seminars();

            //http://localhost:49712/api/schedules/searchbylocation

            //Content-Type: application/json

            //POST
            //{"locationId":54157,"courseId":169,"searchId":""}

            SchedulesSearchRequest searchRequest = JsonConvert.DeserializeObject<SchedulesSearchRequest>(json.ToString());

            if (searchRequest != null && searchRequest.LocationId > 0 && false == string.IsNullOrWhiteSpace(searchRequest.SearchId))
            {
                searchRequest.SearchId = searchRequest.SearchId.ToLower();

                resultsList = seminarsObj.SearchSchedules(searchRequest);
            }


            return resultsList;
        }
    }
}
