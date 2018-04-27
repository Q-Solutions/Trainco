using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using TPCTrainco.Umbraco.Extensions.Models;
using TPCTrainco.Umbraco.Extensions.Models.SearchRequest;
using TPCTrainco.Umbraco.Extensions.Objects;
using TPCTrainco.Umbraco.Extensions.ViewModels;
using TPCTrainco.Umbraco.Extensions.ViewModels.Backbone;

namespace TPCTrainco.Umbraco.Extensions.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ImportController : ApiController
    {
        [HttpGet]
        public object Redirects()
        {
            string output = Helpers.ImportRedirects.Import(HttpContext.Current.Server.MapPath("/data/American-Trainco-URL-Map-FIXED.xlsx"));

            return output;
        }

        [HttpGet]
        public object RedirectsExternal()
        {
            return Helpers.ImportRedirects.ImportExternalRedirects(HttpContext.Current.Server.MapPath("/data/American-Trainco-URL-Map-External.xlsx"));
        } 
    }
}
