using FinanceNotifier.Core;

using System.Diagnostics;

using DotNetEnv;

// Create builder
var builder = WebApplication.CreateBuilder(args);

// Configure host settings
builder.WebHost.UseUrls("http://localhost:5151");
builder.Logging.AddFilter("Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware", LogLevel.None);

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

// Validate required environment variable
var dbConn = Environment.GetEnvironmentVariable("FINANCE_NOTIFIER_CONN_STRING")
    ?? throw new InvalidOperationException("FINANCE_NOTIFIER_CONN_STRING environment variable is required.");

// Add services to DI container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Finance Notifier API",
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
var app = builder.Build();

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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Finance Notifier API v1");
        options.RoutePrefix = "swagger"; // Access at /swagger
    });
}

app.Run();