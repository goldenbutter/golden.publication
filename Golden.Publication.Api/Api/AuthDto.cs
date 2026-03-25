using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Golden.Publication.Api;

public sealed class RegisterRequestDto
{
    [Required]
    [MinLength(3)]
    [MaxLength(50)]
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(4)]
    [MaxLength(100)]
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginRequestDto
{
    [Required]
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public sealed class AuthResponseDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }

    [JsonPropertyName("user")]
    public AuthUserDto User { get; set; } = new();
}

public sealed class AuthUserDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}
