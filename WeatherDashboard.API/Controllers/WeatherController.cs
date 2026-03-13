using Microsoft.AspNetCore.Mvc;
using WeatherDashboard.API.Models;
using WeatherDashboard.API.Services;

namespace WeatherDashboard.API.Controllers;

/// <summary>
/// Provides weather data endpoints for current conditions, forecasts, and city search.
/// </summary>
[ApiController]
[Route("api/weather")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<WeatherController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="WeatherController"/>.
    /// </summary>
    public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current weather conditions for the specified city.
    /// </summary>
    /// <param name="cityName">The name of the city.</param>
    /// <returns>Current weather data wrapped in a standardized API response.</returns>
    /// <response code="200">Returns the current weather data.</response>
    /// <response code="404">City not found.</response>
    [HttpGet("current/{cityName}")]
    [ProducesResponseType(typeof(ApiResponse<WeatherResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WeatherResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentWeather(string cityName)
    {
        _logger.LogInformation("GET /api/weather/current/{CityName}", cityName);
        var data = await _weatherService.GetCurrentWeatherAsync(cityName);
        return Ok(ApiResponse<WeatherResponse>.Ok(data, "Weather data retrieved successfully"));
    }

    /// <summary>
    /// Gets the weather forecast (hourly and 5-day extended) for the specified city.
    /// </summary>
    /// <param name="cityName">The name of the city.</param>
    /// <returns>Forecast data wrapped in a standardized API response.</returns>
    /// <response code="200">Returns the forecast data.</response>
    /// <response code="404">City not found.</response>
    [HttpGet("forecast/{cityName}")]
    [ProducesResponseType(typeof(ApiResponse<ForecastResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ForecastResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForecast(string cityName)
    {
        _logger.LogInformation("GET /api/weather/forecast/{CityName}", cityName);
        var data = await _weatherService.GetForecastAsync(cityName);
        return Ok(ApiResponse<ForecastResponse>.Ok(data, "Forecast data retrieved successfully"));
    }

    /// <summary>
    /// Searches for cities matching the provided query string for autocomplete.
    /// </summary>
    /// <param name="query">The partial city name to search (minimum 2 characters).</param>
    /// <returns>A list of matching city results wrapped in a standardized API response.</returns>
    /// <response code="200">Returns the city search results.</response>
    /// <response code="400">Query is too short.</response>
    [HttpGet("search/{query}")]
    [ProducesResponseType(typeof(ApiResponse<IList<LocationSearchResult>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IList<LocationSearchResult>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchCities(string query)
    {
        _logger.LogInformation("GET /api/weather/search/{Query}", query);

        if (query.Length < 2)
        {
            return BadRequest(ApiResponse<IList<LocationSearchResult>>.Fail("Query must be at least 2 characters."));
        }

        var data = await _weatherService.SearchCitiesAsync(query);
        return Ok(ApiResponse<IList<LocationSearchResult>>.Ok(data, "City search results"));
    }
}
