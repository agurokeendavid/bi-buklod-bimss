namespace Bimss.Contracts.Auth;

// The refresh token is never in this response body — it's set as an
// httpOnly cookie by the server so client-side JS can't read it.
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; set; }
}
