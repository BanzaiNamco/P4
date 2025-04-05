using System.ComponentModel.DataAnnotations;

namespace Auth.Model
{
    public class User
    {
        [Key]
        public string IDno { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;
    }
}