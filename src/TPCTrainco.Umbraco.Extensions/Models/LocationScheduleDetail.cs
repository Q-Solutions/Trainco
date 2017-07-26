using System;
using System.Data.Entity.Spatial;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace TPCTrainco.Umbraco.Extensions.Models
{
    [TableName("CacheLocationScheduleDetail")]
    public class LocationScheduleDetail
    {
        public long Id { get; set; }

        public int CourseId { get; set; }

        public int TopicId { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string City { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string StateCode { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string State { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string LocationDetails { get; set; }

        [ResultColumn]
        public DbGeography CoordinatesObj
        {
            get { return DbGeography.FromText(Coordinates); }
            set { Coordinates = value.AsText(); }
        }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Coordinates { get; set; }


        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime DateFilter { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string DateMonthYear { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public double? Distance { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public long ParentId { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string ScheduleSeminarNumber { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string DaysTitle { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string DaysDescription { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Date { get; set; }

        public double Price { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Description { get; set; }

        public long SeminarId { get; set; }
        public string SeminarTitle { get; set; }
        public string ScheduleType { get; set; }
        public string TrainingKey { get; set; }
    }
}
