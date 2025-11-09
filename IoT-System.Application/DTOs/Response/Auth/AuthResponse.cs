namespace IoT_System.Application.DTOs.Response.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}