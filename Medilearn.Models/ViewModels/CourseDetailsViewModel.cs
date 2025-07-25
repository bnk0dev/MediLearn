﻿namespace Medilearn.Models.ViewModels
{
    public class CourseDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? MaterialPath { get; set; }
    }
}
