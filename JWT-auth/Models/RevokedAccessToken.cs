namespace JWT_auth.Models;

public class RevokedAccessToken
{
    public int Id { get; set; }
    public string Jti { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime RevokedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime AccessTokenExpiresAtUtc { get; set; }
}
