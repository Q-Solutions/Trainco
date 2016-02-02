﻿using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace TPCTrainco.Umbraco.Extensions.Models
{
    [TableName("CacheLocationScheduleDetail")]
    public class LocationScheduleDetail
    {
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

        [NullSetting(NullSetting = NullSettings.Null)]
        public System.Data.Entity.Spatial.DbGeography Coordinates { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime DateFilter { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public double? Distance { get; set; }

        public long Id { get; set; }

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
    }
}
