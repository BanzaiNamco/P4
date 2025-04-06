using System.ComponentModel.DataAnnotations;

namespace Section.Models
{
    public class Section
    {
        [Key]
        public string SectionID { get; set; } = "";
        public string CourseID { get; set; } = "";
        public string ProfID { get; set; } = "";
        public int numStudents { get; set; } = 0;
        public int maxStudents { get; set; } = 0;
    }
}
