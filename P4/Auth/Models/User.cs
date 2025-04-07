using System.ComponentModel.DataAnnotations;

namespace Auth.Models
{
    public class User
    {
        [Key]
        public string IDno { get; set; } = "";

        public string Password { get; set; } = "";
        public string Type { get; set; } = "";

        public User(string iDno, string password, string type)
        {
            IDno = iDno;
            Password = password;
            Type = type;
        }
    }

    public class LoginRequest
    {
        public string IDno { get; set; } = "";
        public string Password { get; set; } = "";
    }

}