namespace L_L.Business.Commons.Response;

public class SignInMobileResponse
{
    public string? message { get; set; }
    public Token? data { get; set; }
}

public class Token
{
    public string? accessToken { get; set; }
    public string? refreshToken { get; set; }
}