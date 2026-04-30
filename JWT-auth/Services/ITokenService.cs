using JWT_auth.DTOs.Auth;
using JWT_auth.Models;

namespace JWT_auth.Services;

public interface ITokenService
{
    Task<AuthResponse> CreateTokensAsync(ApplicationUser user);
}
