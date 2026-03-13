using WeatherDashboard.API.Models;

namespace WeatherDashboard.API.Services;

/// <summary>
/// Provides weather data operations by proxying to the OpenWeatherMap API.
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Gets the current weather conditions for the specified city.
    /// </summary>
    /// <param name="cityName">The name of the city.</param>
    /// <returns>Current weather data for the city.</returns>
    Task<WeatherResponse> GetCurrentWeatherAsync(string cityName);

    /// <summary>
    /// Gets the weather forecast (hourly + 5-day) for the specified city.
    /// </summary>
    /// <param name="cityName">The name of the city.</param>
    /// <returns>Forecast data for the city.</returns>
    Task<ForecastResponse> GetForecastAsync(string cityName);

    /// <summary>
    /// Searches for cities matching the given query string for autocomplete.
    /// </summary>
    /// <param name="query">The partial city name to search for.</param>
    /// <returns>A list of matching city results.</returns>
    Task<IList<LocationSearchResult>> SearchCitiesAsync(string query);
}
