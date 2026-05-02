using System.Security.Claims;
using JWT_auth.Data;
using JWT_auth.Models;
using Microsoft.EntityFrameworkCore;

namespace JWT_auth.Middleware;

public class AuditLoggingMiddleware
{
    private static readonly string[] SkippedPathPrefixes = { "/swagger" };
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;

    public AuditLoggingMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        var path = context.Request.Path.Value ?? string.Empty;
        if (SkippedPathPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Endpoint = path,
            HttpMethod = context.Request.Method,
            OccurredAtUtc = DateTime.UtcNow
        });

        try
        {
            await db.SaveChangesAsync();
        }
        catch
        {
            // Do not fail the response if audit persistence fails
        }
    }
}
