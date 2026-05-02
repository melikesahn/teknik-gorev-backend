using System.IdentityModel.Tokens.Jwt;
using JWT_auth.Data;
using JWT_auth.DTOs.Auth;
using JWT_auth.Models;
using JWT_auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JWT_auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Individual",
        "Corporate",
        "Admin"
    };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!AllowedRoles.Contains(request.Role))
        {
            return BadRequest("Role must be one of: Individual, Corporate, Admin.");
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return BadRequest("Email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
        {
            return BadRequest(roleResult.Errors.Select(e => e.Description));
        }

        return Ok("User registered.");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized("Invalid credentials.");
        }

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
        {
            return Unauthorized("Invalid credentials.");
        }

        var tokens = await _tokenService.CreateTokensAsync(user);
        return Ok(tokens);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        var tokenInDb = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (tokenInDb is null || !tokenInDb.IsActive)
        {
            return Unauthorized("Invalid refresh token.");
        }

        tokenInDb.RevokedAtUtc = DateTime.UtcNow;
        var user = await _userManager.FindByIdAsync(tokenInDb.UserId);
        if (user is null)
        {
            return Unauthorized("User not found.");
        }

        var newTokens = await _tokenService.CreateTokensAsync(user);
        await _dbContext.SaveChangesAsync();

        return Ok(newTokens);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(RefreshRequest request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);
        if (!string.IsNullOrEmpty(jti) && long.TryParse(expClaim, out var expUnix))
        {
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            if (!await _dbContext.RevokedAccessTokens.AnyAsync(x => x.Jti == jti))
            {
                _dbContext.RevokedAccessTokens.Add(new RevokedAccessToken
                {
                    Jti = jti,
                    UserId = currentUserId,
                    AccessTokenExpiresAtUtc = expiresAt
                });
            }
        }

        var tokenInDb = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (tokenInDb is null || tokenInDb.IsRevoked)
        {
            await _dbContext.SaveChangesAsync();
            return Ok("Already logged out.");
        }

        if (tokenInDb.UserId != currentUserId)
        {
            await _dbContext.SaveChangesAsync();
            return Forbid();
        }

        tokenInDb.RevokedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok("Logged out.");
    }
}
