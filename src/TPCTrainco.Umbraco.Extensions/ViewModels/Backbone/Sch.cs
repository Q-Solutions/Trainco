﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPCTrainco.Umbraco.Extensions.ViewModels.Backbone
{
    public class Sch
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int CityId { get; set; }
        public string DaysTitle { get; set; }
        public string DaysDescription { get; set; }
        public string Date { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
    }
}
