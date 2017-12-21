using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPCTrainco.Umbraco.Extensions.ViewModels.Search
{
    public class LocationSchedule
    {
        public string City { get; set; }
        public string StateCode { get; set; }
        public string State { get; set; }
        public string LocationDetails { get; set; }
        public DateTime DateFilter { get; set; }
        public string DateMonthYear { get; set; }
        public double Distance { get; set; }

        public long Id { get; set; }
        public string DaysTitle { get; set; }
        public string DaysDescription { get; set; }
        public string Date { get; set; }
        public double Price { get; set; }

        public long SeminarId { get; set; }
        public string SeminarTitle { get; set; }
        public string RegisterUri { get; set; }
        public string ViewUri { get; set; }
    }
}
