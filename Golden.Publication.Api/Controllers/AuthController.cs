using System.Security.Claims;
using Golden.Publication.Api.Domain;
using Golden.Publication.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Golden.Publication.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly AuthSettings _settings;

    public AuthController(AuthService authService, IOptions<AuthSettings> options)
    {
        _authService = authService;
        _settings = options.Value;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString());
            AppendRefreshCookie(result.RefreshToken);
            return Ok(result.Response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Title = "Registration failed", Detail = ex.Message, Status = StatusCodes.Status409Conflict });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString());
        if (result is null)
            return Unauthorized(new ProblemDetails { Title = "Login failed", Detail = "Invalid username or password.", Status = StatusCodes.Status401Unauthorized });

        AppendRefreshCookie(result.Value.RefreshToken);
        return Ok(result.Value.Response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[_settings.RefreshCookieName];
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new ProblemDetails { Title = "Refresh failed", Detail = "Missing refresh token.", Status = StatusCodes.Status401Unauthorized });

        var result = await _authService.RefreshAsync(refreshToken, HttpContext.Connection.RemoteIpAddress?.ToString());
        if (result is null)
            return Unauthorized(new ProblemDetails { Title = "Refresh failed", Detail = "Invalid or expired refresh token.", Status = StatusCodes.Status401Unauthorized });

        AppendRefreshCookie(result.Value.RefreshToken);
        return Ok(result.Value.Response);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[_settings.RefreshCookieName];
        if (!string.IsNullOrWhiteSpace(refreshToken))
            await _authService.RevokeRefreshTokenAsync(refreshToken, HttpContext.Connection.RemoteIpAddress?.ToString());

        DeleteRefreshCookie();
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _authService.GetUserAsync(userId);
        if (user is null)
            return Unauthorized();

        return Ok(user);
    }

    private void AppendRefreshCookie(string refreshToken)
    {
        Response.Cookies.Append(_settings.RefreshCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(_settings.RefreshTokenHours),
            Path = "/",
            IsEssential = true
        });
    }

    private void DeleteRefreshCookie()
    {
        Response.Cookies.Delete(_settings.RefreshCookieName, new CookieOptions
        {
            Path = "/"
        });
    }
}
