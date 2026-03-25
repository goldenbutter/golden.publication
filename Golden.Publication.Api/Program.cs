using Golden.Publication.Api.Domain;
using Golden.Publication.Api.Infrastructure;
using Golden.Publication.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));
var authSettings = builder.Configuration.GetSection("Auth").Get<AuthSettings>() ?? new AuthSettings();
if (builder.Environment.EnvironmentName != "Testing" && string.IsNullOrWhiteSpace(authSettings.Key))
{
    throw new InvalidOperationException("Auth:Key configuration is required.");
}

// Conditional CORS (based on appsettings)
var enableCors = builder.Configuration.GetValue<bool>("EnableCors");
if (enableCors)
{
    var allowed = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

    const string AllowClient = "AllowClient";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(AllowClient, policy =>
        {
            policy.WithOrigins(allowed)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });
}

// PostgreSQL via EF Core — connection string supplied via env var:
//   ConnectionStrings__Publications="Host=...;Database=...;Username=...;Password=..."
if (builder.Environment.EnvironmentName != "Testing")
{
    builder.Services.AddDbContext<PublicationDbContext>(opts =>
        opts.UseNpgsql(builder.Configuration.GetConnectionString("Publications")));
}

builder.Services.AddScoped<IPublicationRepository, EfPublicationRepository>();
builder.Services.AddScoped<PublicationService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var settingsScope = builder.Configuration.GetSection("Auth").Get<AuthSettings>() ?? new AuthSettings();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = settingsScope.Issuer,
            ValidAudience = settingsScope.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settingsScope.Key ?? "")),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Migrate DB schema and seed from XML on every cold start (idempotent)
using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    if (!env.IsEnvironment("Testing"))
    {
        var ctx = scope.ServiceProvider.GetRequiredService<PublicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        await DatabaseSeeder.SeedAsync(ctx, env, logger);
    }
}

// Swagger
app.UseSwagger(c => { c.RouteTemplate = "swagger/{documentName}/swagger.json"; });
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Publications API v1");
    c.RoutePrefix = "swagger";
});

if (enableCors)
    app.UseCors("AllowClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

public partial class Program { }
