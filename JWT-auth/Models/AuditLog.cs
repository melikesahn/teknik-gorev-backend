namespace JWT_auth.Models;

public class AuditLog
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}
