using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPCTrainco.Umbraco.Extensions.ViewModels.Search
{
    public class Seminar
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string ImageUrl { get; set; }
        public string DetailsUrl { get; set; }
        public double Price { get; set; }
        public int PageTotal { get; set; }
        public List<LocationSchedule> LocationSchedules { get; set; }
        public List<LocationSchedule> SimulcastSchedules { get; set; }
    }
}
