using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace TPCTrainco.Umbraco.Extensions.Models
{
    [TableName("CacheCourseDetail")]
    public class CourseDetail
    {
        public int Id { get; set; }

        public int TopicId { get; set; }

        public int CourseTier { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string Title { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string SubTitle { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string ImageUrl { get; set; }

        [NullSetting(NullSetting = NullSettings.Null)]
        public string DetailsUrl { get; set; }

        //[NullSetting(NullSetting = NullSettings.Null)]
        //[Ignore]
        //public int NodeId { get; set; }

        public double Price { get; set; }


        
    }
}
