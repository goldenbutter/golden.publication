using Golden.Publication.Api.Infrastructure;
using Golden.Publication.Data;
using Golden.Publication.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Golden.Publication.Api.Domain;

public sealed class AuthService
{
    private readonly PublicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly AuthSettings _settings;

    public AuthService(
        PublicationDbContext db,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IOptions<AuthSettings> settings)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _settings = settings.Value;
    }

    public async Task<(AuthResponseDto Response, string RefreshToken)> RegisterAsync(RegisterRequestDto request, string? ipAddress)
    {
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var exists = await _db.Users.AnyAsync(x => x.Username.ToLower() == normalizedUsername);
        if (exists)
            throw new InvalidOperationException("Username already exists.");

        var now = DateTimeOffset.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = "user",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            LastLoginAt = now
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return await IssueSessionAsync(user, ipAddress, revokeExisting: true);
    }

    public async Task<(AuthResponseDto Response, string RefreshToken)?> LoginAsync(LoginRequestDto request, string? ipAddress)
    {
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Username.ToLower() == request.Username.Trim().ToLower());
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash) || !user.IsActive)
            return null;

        return await IssueSessionAsync(user, ipAddress, revokeExisting: true);
    }

    public async Task<(AuthResponseDto Response, string RefreshToken)?> RefreshAsync(string refreshToken, string? ipAddress)
    {
        var tokenHash = _tokenService.HashRefreshToken(refreshToken);
        var stored = await _db.RefreshTokens
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash);

        if (stored is null || stored.User is null || stored.RevokedAt is not null || stored.ExpiresAt <= DateTimeOffset.UtcNow)
            return null;

        var newRefreshToken = _tokenService.CreateRefreshToken();
        var newRefreshTokenHash = _tokenService.HashRefreshToken(newRefreshToken);
        stored.RevokedAt = DateTimeOffset.UtcNow;
        stored.RevokedByIp = ipAddress;
        stored.ReplacedByTokenHash = newRefreshTokenHash;

        var replacement = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = stored.UserId,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(_settings.RefreshTokenHours),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByIp = ipAddress,
        };
        _db.RefreshTokens.Add(replacement);

        var (accessToken, accessExpiresAt) = _tokenService.CreateAccessToken(stored.User);
        stored.User.LastLoginAt = DateTimeOffset.UtcNow;
        stored.User.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();

        return (new AuthResponseDto
        {
            AccessToken = accessToken,
            ExpiresAt = accessExpiresAt,
            User = new AuthUserDto
            {
                Id = stored.User.Id.ToString(),
                Username = stored.User.Username,
                Role = stored.User.Role
            }
        }, newRefreshToken);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, string? ipAddress)
    {
        var tokenHash = _tokenService.HashRefreshToken(refreshToken);
        var stored = await _db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == tokenHash);
        if (stored is null || stored.RevokedAt is not null)
            return false;

        stored.RevokedAt = DateTimeOffset.UtcNow;
        stored.RevokedByIp = ipAddress;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<AuthUserDto?> GetUserAsync(Guid userId)
    {
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == userId && x.IsActive);
        if (user is null)
            return null;

        return new AuthUserDto
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            Role = user.Role
        };
    }

    private async Task<(AuthResponseDto Response, string RefreshToken)> IssueSessionAsync(User user, string? ipAddress, bool revokeExisting)
    {
        if (revokeExisting)
        {
            var activeTokens = await _db.RefreshTokens
                .Where(x => x.UserId == user.Id && x.RevokedAt == null && x.ExpiresAt > DateTimeOffset.UtcNow)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTimeOffset.UtcNow;
                token.RevokedByIp = ipAddress;
            }
        }

        var refreshToken = _tokenService.CreateRefreshToken();
        var refreshTokenHash = _tokenService.HashRefreshToken(refreshToken);
        var refreshEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(_settings.RefreshTokenHours),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByIp = ipAddress,
        };

        _db.RefreshTokens.Add(refreshEntity);
        var (accessToken, accessExpiresAt) = _tokenService.CreateAccessToken(user);
        user.LastLoginAt = DateTimeOffset.UtcNow;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();

        return (new AuthResponseDto
        {
            AccessToken = accessToken,
            ExpiresAt = accessExpiresAt,
            User = new AuthUserDto
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Role = user.Role
            }
        }, refreshToken);
    }
}
