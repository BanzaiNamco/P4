using System.ComponentModel.DataAnnotations;

namespace Course.Models
{
    public class Course
    {
        [Key]
        public string CourseID { get; set; } = "";

        public string CourseName { get; set; } = "";
    }

}