using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using TPCTrainco.Umbraco.Extensions.Objects;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace TPCTrainco.Umbraco.Extensions.Controllers
{
    [PluginController("TPCTrainco")]
    public class CitiesController : UmbracoAuthorizedJsonController
    {
        public IEnumerable<object> GetCities()
        {
            List<object> data = new List<object>();
            List<SeminarCitiesActive> dtList = CacheObjects.GetAcitveCitiesList().OrderBy(x => x.CityName).ToList();
            foreach (SeminarCitiesActive oObj in dtList)
            {
                var oCity = new { CityID = oObj.CityID, CityName = oObj.CityName, State = oObj.StateAbbrv, Title = oObj.CityName + ", " + oObj.StateAbbrv };
                data.Add(oCity);
            }
            return data;
        }
    }
}
