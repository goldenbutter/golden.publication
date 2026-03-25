using Golden.Publication.Api;
using Golden.Publication.Api.Domain;
using Golden.Publication.Api.Infrastructure;
using Golden.Publication.Data;
using Golden.Publication.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Golden.Publication.Tests;

public class AuthServiceTests
{
    private readonly DbContextOptions<PublicationDbContext> _dbOptions;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IOptions<AuthSettings>> _authSettingsMock;
    private readonly AuthSettings _authSettings;

    public AuthServiceTests()
    {
        _dbOptions = new DbContextOptionsBuilder<PublicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();
        _authSettingsMock = new Mock<IOptions<AuthSettings>>();

        _authSettings = new AuthSettings
        {
            AccessTokenHours = 1,
            RefreshTokenHours = 24,
            Key = "test-key-must-be-long-enough-for-hmac-sha256",
            Issuer = "test-issuer",
            Audience = "test-audience",
            RefreshCookieName = "test-refresh-token"
        };

        _authSettingsMock.Setup(x => x.Value).Returns(_authSettings);
    }

    private PublicationDbContext CreateContext() => new PublicationDbContext(_dbOptions);

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenUsernameIsUnique()
    {
        // Arrange
        using var db = CreateContext();
        var service = new AuthService(db, _passwordHasherMock.Object, _tokenServiceMock.Object, _authSettingsMock.Object);
        var request = new RegisterRequestDto { Username = "newuser", Password = "password123" };
        
        _passwordHasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed-password");
        _tokenServiceMock.Setup(x => x.CreateAccessToken(It.IsAny<User>()))
            .Returns(("access-token", DateTimeOffset.UtcNow.AddHours(1)));
        _tokenServiceMock.Setup(x => x.CreateRefreshToken()).Returns("refresh-token");
        _tokenServiceMock.Setup(x => x.HashRefreshToken(It.IsAny<string>())).Returns("hashed-refresh-token");

        // Act
        var (response, refreshToken) = await service.RegisterAsync(request, "127.0.0.1");

        // Assert
        Assert.NotNull(response);
        Assert.Equal("newuser", response.User.Username);
        Assert.Equal("refresh-token", refreshToken);
        
        var user = await db.Users.SingleOrDefaultAsync(u => u.Username == "newuser");
        Assert.NotNull(user);
        Assert.Equal("hashed-password", user.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenUsernameExists()
    {
        // Arrange
        using var db = CreateContext();
        db.Users.Add(new User { Id = Guid.NewGuid(), Username = "existinguser", PasswordHash = "hash", Role = "user", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow, IsActive = true });
        await db.SaveChangesAsync();

        var service = new AuthService(db, _passwordHasherMock.Object, _tokenServiceMock.Object, _authSettingsMock.Object);
        var request = new RegisterRequestDto { Username = "existinguser", Password = "password123" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(request, "127.0.0.1"));
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        using var db = CreateContext();
        var user = new User 
        { 
            Id = Guid.NewGuid(), 
            Username = "testuser", 
            PasswordHash = "hashed-password", 
            Role = "user", 
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow, 
            UpdatedAt = DateTimeOffset.UtcNow 
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new AuthService(db, _passwordHasherMock.Object, _tokenServiceMock.Object, _authSettingsMock.Object);
        var request = new LoginRequestDto { Username = "testuser", Password = "password123" };

        _passwordHasherMock.Setup(x => x.Verify("password123", "hashed-password")).Returns(true);
        _tokenServiceMock.Setup(x => x.CreateAccessToken(It.IsAny<User>()))
            .Returns(("access-token", DateTimeOffset.UtcNow.AddHours(1)));
        _tokenServiceMock.Setup(x => x.CreateRefreshToken()).Returns("refresh-token");
        _tokenServiceMock.Setup(x => x.HashRefreshToken("refresh-token")).Returns("hashed-refresh-token");

        // Act
        var result = await service.LoginAsync(request, "127.0.0.1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.Value.Response.AccessToken);
        Assert.Equal("refresh-token", result.Value.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenCredentialsAreInvalid()
    {
        // Arrange
        using var db = CreateContext();
        db.Users.Add(new User { Id = Guid.NewGuid(), Username = "testuser", PasswordHash = "hashed-password", Role = "user", IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var service = new AuthService(db, _passwordHasherMock.Object, _tokenServiceMock.Object, _authSettingsMock.Object);
        var request = new LoginRequestDto { Username = "testuser", Password = "wrongpassword" };

        _passwordHasherMock.Setup(x => x.Verify("wrongpassword", "hashed-password")).Returns(false);

        // Act
        var result = await service.LoginAsync(request, "127.0.0.1");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshAsync_ShouldRotateToken_WhenValid()
    {
        // Arrange
        using var db = CreateContext();
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", PasswordHash = "hash", Role = "user", IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
        db.Users.Add(user);
        
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "old-token-hash",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync();

        var service = new AuthService(db, _passwordHasherMock.Object, _tokenServiceMock.Object, _authSettingsMock.Object);
        
        _tokenServiceMock.Setup(x => x.HashRefreshToken("old-token")).Returns("old-token-hash");
        _tokenServiceMock.Setup(x => x.CreateRefreshToken()).Returns("new-token");
        _tokenServiceMock.Setup(x => x.HashRefreshToken("new-token")).Returns("new-token-hash");
        _tokenServiceMock.Setup(x => x.CreateAccessToken(It.IsAny<User>()))
            .Returns(("new-access-token", DateTimeOffset.UtcNow.AddHours(1)));

        // Act
        var result = await service.RefreshAsync("old-token", "127.0.0.1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new-access-token", result.Value.Response.AccessToken);
        Assert.Equal("new-token", result.Value.RefreshToken);

        var oldToken = await db.RefreshTokens.SingleAsync(t => t.TokenHash == "old-token-hash");
        Assert.NotNull(oldToken.RevokedAt);
        Assert.Equal("new-token-hash", oldToken.ReplacedByTokenHash);

        var newToken = await db.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == "new-token-hash");
        Assert.NotNull(newToken);
    }
}
