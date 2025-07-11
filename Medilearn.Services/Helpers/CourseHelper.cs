using Medilearn.Data.Entities;
using System;

namespace MediLearn.Services.Helpers
{
    public static class CourseHelper
    {
        public static bool IsPast(Course course)
        {
            return course.EndDate < DateTime.Now;
        }

        public static bool IsOngoing(Course course)
        {
            return course.StartDate <= DateTime.Now && course.EndDate >= DateTime.Now;
        }

        public static bool IsUpcoming(Course course)
        {
            return course.StartDate > DateTime.Now;
        }
    }
}
