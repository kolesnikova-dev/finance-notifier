using FinanceScraper.Core;

using System.Diagnostics;

using Microsoft.Data.Sqlite;

using DotNetEnv;
// Load environment variables
if (Debugger.IsAttached)
{
    string envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
    if (File.Exists(envPath))
    {
        Env.Load(envPath);
    }
}
else
{
    Env.Load();
}
// I will add proper variable validation next ------------------------------------------
// aspNetCoreUrls is a placeholder for future full stack links
string aspNetCoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")
    ?? throw new InvalidOperationException("ASPNETCORE_URLS environment variable is required.");
var port = Environment.GetEnvironmentVariable("PORT");
string environment = Environment.GetEnvironmentVariable("ENVIRONMENT")
 ?? throw new InvalidOperationException("ENVIRONMENT environment variable is required.");

using var connection = new SqliteConnection($"Data Source=finance_scraper_{environment}.db");
connection.Open();


// Create builder
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure host settings
builder.Logging.AddFilter("Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware", LogLevel.None);

if (!string.IsNullOrEmpty(port))
{
    // For Azure/Railway/Heroku
    builder.WebHost.UseUrls($"http://*:{port}");
}
else if (aspNetCoreUrls != null)
{
    // Use environment variable if set
    builder.WebHost.UseUrls(aspNetCoreUrls);
}



// Add services to DI container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Finance Scraper API",
        Version = "v1",
        Description = "API for scraping and summarizing financial articles"
    });
});

// Register Core services
builder.Services.AddScoped<ISummarizer, GeminiFlashSummarizer>();
builder.Services.AddScoped<IUrlValidator, UrlValidator>();
builder.Services.AddScoped<IDataScraperFormatter>(sp =>
    new DataScraperFormatter(ScraperUrls.FinanceUrls));

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Build application
WebApplication app = builder.Build();

// CORS configuration first
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.MapControllers();

// Configure the HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Finance Scraper API v1");
        options.RoutePrefix = "swagger"; // Access at /swagger
    });
}

app.Run();