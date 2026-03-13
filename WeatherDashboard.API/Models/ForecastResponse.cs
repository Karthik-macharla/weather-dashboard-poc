namespace WeatherDashboard.API.Models;

/// <summary>
/// Hourly forecast data point.
/// </summary>
public class HourlyForecast
{
    /// <summary>Gets or sets the forecast time (UTC).</summary>
    public DateTime Time { get; set; }

    /// <summary>Gets or sets the temperature.</summary>
    public Temperature Temperature { get; set; } = new();

    /// <summary>Gets or sets the precipitation chance as a percentage.</summary>
    public int PrecipitationChance { get; set; }

    /// <summary>Gets or sets the wind speed in km/h.</summary>
    public double WindSpeed { get; set; }

    /// <summary>Gets or sets the weather description.</summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Daily forecast data point.
/// </summary>
public class DailyForecast
{
    /// <summary>Gets or sets the forecast date.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Gets or sets the high temperature.</summary>
    public Temperature High { get; set; } = new();

    /// <summary>Gets or sets the low temperature.</summary>
    public Temperature Low { get; set; } = new();

    /// <summary>Gets or sets the precipitation chance as a percentage.</summary>
    public int PrecipitationChance { get; set; }

    /// <summary>Gets or sets the weather description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the weather icon identifier.</summary>
    public string Icon { get; set; } = string.Empty;
}

/// <summary>
/// Full forecast response data including hourly and daily forecasts.
/// </summary>
public class ForecastResponse
{
    /// <summary>Gets or sets the location information.</summary>
    public LocationInfo Location { get; set; } = new();

    /// <summary>Gets or sets today's hourly forecast.</summary>
    public IList<HourlyForecast> TodayHourly { get; set; } = new List<HourlyForecast>();

    /// <summary>Gets or sets the 5-day extended daily forecast.</summary>
    public IList<DailyForecast> ExtendedDaily { get; set; } = new List<DailyForecast>();
}
