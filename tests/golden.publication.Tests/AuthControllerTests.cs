using System.Net;
using System.Net.Http.Json;
using Golden.Publication.Api;
using Golden.Publication.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Golden.Publication.Tests;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Auth:Key"] = "test-key-must-be-long-enough-for-hmac-sha256",
                    ["Auth:Issuer"] = "test-issuer",
                    ["Auth:Audience"] = "test-audience",
                    ["Auth:AccessTokenHours"] = "1",
                    ["Auth:RefreshTokenHours"] = "24",
                    ["Auth:RefreshCookieName"] = "test-refresh-token"
                });
            });
            builder.ConfigureServices(services =>
            {
                // Replace real DbContext with InMemory
                services.RemoveAll<DbContextOptions<PublicationDbContext>>();
                services.AddDbContext<PublicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb");
                });

                // Ensure schema is created
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PublicationDbContext>();
                db.Database.EnsureCreated();
            });
        });
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenValid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RegisterRequestDto { Username = "int_test_user", Password = "password123" };

        // Act
        var response = await client.PostAsJsonAsync("/auth/register", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("int_test_user", result.User.Username);
    }

    [Fact]
    public async Task Login_ReturnsOk_AfterRegister()
    {
        // Arrange
        var client = _factory.CreateClient();
        var registerRequest = new RegisterRequestDto { Username = "login_test_user", Password = "password123" };
        await client.PostAsJsonAsync("/auth/register", registerRequest);

        var loginRequest = new LoginRequestDto { Username = "login_test_user", Password = "password123" };

        // Act
        var response = await client.PostAsJsonAsync("/auth/login", loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("login_test_user", result.User.Username);
        
        // Should have set refresh cookie
        Assert.True(response.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task GetMe_ReturnsUnauthorized_WhenNoToken()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_ReturnsUser_WhenAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var registerRequest = new RegisterRequestDto { Username = "me_test_user", Password = "password123" };
        var regResponse = await client.PostAsJsonAsync("/auth/register", registerRequest);
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regResult!.AccessToken);

        // Act
        var response = await client.GetAsync("/auth/me");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthUserDto>();
        Assert.NotNull(result);
        Assert.Equal("me_test_user", result.Username);
    }
}
