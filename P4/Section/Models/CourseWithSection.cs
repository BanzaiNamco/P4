namespace Section.Models
{
    public class CourseWithSection
    {
        public String CourseID { get; set; } = "";
        public List<String> sections { get; set; } = new List<String>();
    }
}
