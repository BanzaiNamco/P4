public class CourseWithSectionModel
{
    public CourseModel Course { get; set; } = new CourseModel();
    public List<SectionModel> Sections { get; set; } = new List<SectionModel>();
}