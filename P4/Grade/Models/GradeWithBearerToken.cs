namespace Grade.Models
{
    public class GradeWithBearerToken
    {
        public string BearerToken { get; set; } = "";
        public Grade Grade { get; set; } = new Grade();

    }
}
