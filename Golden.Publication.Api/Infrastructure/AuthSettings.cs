namespace Golden.Publication.Api.Infrastructure;

public sealed class AuthSettings
{
    public string Issuer { get; set; } = "Golden.Publication.Api";
    public string Audience { get; set; } = "Golden.Publication.Client";
    public string Key { get; set; } = string.Empty;
    public int AccessTokenHours { get; set; } = 24;
    public int RefreshTokenHours { get; set; } = 24;
    public string RefreshCookieName { get; set; } = "gp_refresh_token";
}
