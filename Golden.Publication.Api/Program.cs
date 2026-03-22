using Golden.Publication.Api.Domain;
using Golden.Publication.Api.Infrastructure;
using Golden.Publication.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
                  .AllowAnyMethod();
        });
    });
}

// PostgreSQL via EF Core — connection string supplied via env var:
//   ConnectionStrings__Publications="Host=...;Database=...;Username=...;Password=..."
builder.Services.AddDbContext<PublicationDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Publications")));

builder.Services.AddScoped<IPublicationRepository, EfPublicationRepository>();
builder.Services.AddScoped<PublicationService>();

var app = builder.Build();

// Migrate DB schema and seed from XML on every cold start (idempotent)
using (var scope = app.Services.CreateScope())
{
    var ctx    = scope.ServiceProvider.GetRequiredService<PublicationDbContext>();
    var env    = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DatabaseSeeder.SeedAsync(ctx, env, logger);
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

app.MapControllers();
app.Run();
