using WeatherDashboard.API.Middleware;
using WeatherDashboard.API.Models;
using WeatherDashboard.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Bind configuration sections
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SectionName));
builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection(CorsSettings.SectionName));

// CORS — restrict to the configured Angular frontend origin(s)
var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>()
    ?? new CorsSettings();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (corsSettings.AllowedOrigins.Length > 0)
        {
            policy.WithOrigins(corsSettings.AllowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // Development fallback — not used in production
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// HTTP client for OpenWeatherMap
builder.Services.AddHttpClient("OpenWeatherMap");

// Register services
builder.Services.AddScoped<IWeatherService, WeatherService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Weather Dashboard API", Version = "v1" });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Global exception handling — must be first in the pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Expose Program class for integration testing
public partial class Program { }
