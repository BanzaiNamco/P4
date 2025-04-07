using System.ComponentModel.DataAnnotations;

namespace Grade.Models
{
    public class Grade
    {
        [Key]
        public string GradeID { get; set; } = "";
        public string SectionID { get; set; } = "";
        public string CourseID { get; set; } = "";
        public string StudentID { get; set; } = "";
        public double GradeValue { get; set; } = 0.0;

    }
}
