using Microchip.Interview.Api.Domain;
using Microchip.Interview.Data;

var builder = WebApplication.CreateBuilder(args);


// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === CORS: allow the React dev server (http://localhost:5173) ===
const string AllowClient = "AllowClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowClient, policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// === Registered XML-backed repository ===
builder.Services.AddSingleton<IPublicationRepository>(sp =>
{
    var env = sp.GetRequiredService<IHostEnvironment>();

    var fromEnv = Environment.GetEnvironmentVariable("PUBLICATION_XML_FILE");
    if (!string.IsNullOrWhiteSpace(fromEnv) && File.Exists(fromEnv))
    {
        Console.WriteLine($"[Startup] Using PUBLICATION_XML_FILE={fromEnv}");
        return new XmlPublicationRepository(fromEnv);
    }

    var solutionRoot = Path.GetFullPath(Path.Combine(env.ContentRootPath, ".."));
    var xmlPath = Path.Combine(solutionRoot, "src", "Microchip.Interview.Data", "Data", "publications.xml");

    Console.WriteLine($"[Startup] publications.xml path: {xmlPath}");

    return new XmlPublicationRepository(xmlPath);
});

// Registered domain service
builder.Services.AddScoped<PublicationService>();

var app = builder.Build();

// Swagger

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Publications API v1");
    c.RoutePrefix = string.Empty; // Serve Swagger at root
});


// === Enable CORS ===
app.UseCors(AllowClient);

//app.UseHttpsRedirection();
app.MapControllers();

app.Run();
