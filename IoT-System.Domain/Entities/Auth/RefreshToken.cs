namespace IoT_System.Domain.Entities.Auth;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime ExpiryDate { get; set; }
}