namespace Auth.Model;
public class LoginResponseModel
{
    public string? IDno { get; set; }
    public string? AccessToken { get; set; }
    public int ExpiresIn { get; set; }
}
