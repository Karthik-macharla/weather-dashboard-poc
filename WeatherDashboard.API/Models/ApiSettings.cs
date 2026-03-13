namespace WeatherDashboard.API.Models;

/// <summary>
/// Configuration settings for the OpenWeatherMap API.
/// </summary>
public class ApiSettings
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "WeatherApi";

    /// <summary>Gets or sets the base URL for the OpenWeatherMap API.</summary>
    public string BaseUrl { get; set; } = "https://api.openweathermap.org/data/2.5";

    /// <summary>Gets or sets the OpenWeatherMap API key (loaded from configuration or Azure Key Vault in production).</summary>
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// Configuration settings for CORS.
/// </summary>
public class CorsSettings
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Cors";

    /// <summary>Gets or sets the allowed frontend origins.</summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}
