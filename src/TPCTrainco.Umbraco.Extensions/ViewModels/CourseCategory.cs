using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPCTrainco.Umbraco.Extensions.Objects;

namespace TPCTrainco.Umbraco.Extensions.ViewModels
{
    public class CourseCategory
    {
        public int Id { get; set; }
        public string Title { get; set; }

        private List<Course> _Courses = null;

        public List<Course> Courses
        {
            get 
            {
                Seminars seminarsObj = new Seminars();

                List<COURS> courseList = CacheObjects.GetCourseList();

                if (courseList != null)
                {
                    courseList = courseList.Where(p => p.Active == 1 && p.CourseTopicID == Id).ToList();

                    if (courseList != null)
                    {
                        _Courses = new List<Course>();

                        foreach(COURS courseLegacy in courseList)
                        {
                            Course course = seminarsObj.GetCourseById(courseLegacy.CourseID);

                            if (course != null)
                            {
                                _Courses.Add(course);
                            }
                        }
                    }
                }

                return _Courses; 
            }
        }
    }
}
