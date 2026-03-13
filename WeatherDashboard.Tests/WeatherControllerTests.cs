using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WeatherDashboard.API.Controllers;
using WeatherDashboard.API.Models;
using WeatherDashboard.API.Services;
using Xunit;

namespace WeatherDashboard.Tests;

public class WeatherControllerTests
{
    private readonly Mock<IWeatherService> _mockService = new();
    private readonly WeatherController _controller;

    public WeatherControllerTests()
    {
        _controller = new WeatherController(_mockService.Object, NullLogger<WeatherController>.Instance);
    }

    // ── GetCurrentWeather ────────────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentWeather_ValidCity_Returns200WithData()
    {
        var expectedData = new WeatherResponse
        {
            Location = new LocationInfo { City = "Toronto", Country = "CA" },
            Current = new CurrentConditions
            {
                Temperature = new Temperature { Celsius = 22, Fahrenheit = 71.6 },
                Description = "Clear sky"
            }
        };
        _mockService.Setup(s => s.GetCurrentWeatherAsync("Toronto"))
                    .ReturnsAsync(expectedData);

        var result = await _controller.GetCurrentWeather("Toronto");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<WeatherResponse>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Weather data retrieved successfully", response.Message);
        Assert.Equal("Toronto", response.Data!.Location.City);
    }

    [Fact]
    public async Task GetCurrentWeather_ServiceThrowsKeyNotFound_PropagatesException()
    {
        _mockService.Setup(s => s.GetCurrentWeatherAsync("Unknown"))
                    .ThrowsAsync(new KeyNotFoundException("City 'Unknown' not found."));

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _controller.GetCurrentWeather("Unknown"));
    }

    // ── GetForecast ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetForecast_ValidCity_Returns200WithData()
    {
        var expectedData = new ForecastResponse
        {
            Location = new LocationInfo { City = "London", Country = "GB" },
            TodayHourly = new List<HourlyForecast>
            {
                new() { Temperature = new Temperature { Celsius = 18 }, Description = "Cloudy" }
            },
            ExtendedDaily = new List<DailyForecast>
            {
                new() { Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), Description = "Sunny" }
            }
        };
        _mockService.Setup(s => s.GetForecastAsync("London"))
                    .ReturnsAsync(expectedData);

        var result = await _controller.GetForecast("London");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<ForecastResponse>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Forecast data retrieved successfully", response.Message);
        Assert.Equal("London", response.Data!.Location.City);
        Assert.Single(response.Data.TodayHourly);
    }

    [Fact]
    public async Task GetForecast_ServiceThrowsKeyNotFound_PropagatesException()
    {
        _mockService.Setup(s => s.GetForecastAsync("Unknown"))
                    .ThrowsAsync(new KeyNotFoundException("City 'Unknown' not found."));

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _controller.GetForecast("Unknown"));
    }

    // ── SearchCities ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchCities_ValidQuery_Returns200WithResults()
    {
        var results = new List<LocationSearchResult>
        {
            new() { CityName = "Toronto", Country = "Canada", DisplayName = "Toronto, Ontario, Canada" }
        };
        _mockService.Setup(s => s.SearchCitiesAsync("Tor"))
                    .ReturnsAsync(results);

        var result = await _controller.SearchCities("Tor");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IList<LocationSearchResult>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Single(response.Data!);
        Assert.Equal("Toronto", response.Data![0].CityName);
    }

    [Fact]
    public async Task SearchCities_SingleCharQuery_Returns400()
    {
        var result = await _controller.SearchCities("T");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IList<LocationSearchResult>>>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Contains("at least 2 characters", response.Message);
    }

    [Fact]
    public async Task SearchCities_EmptyQuery_Returns400()
    {
        var result = await _controller.SearchCities("");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IList<LocationSearchResult>>>(badRequest.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task SearchCities_EmptyResults_Returns200WithEmptyList()
    {
        _mockService.Setup(s => s.SearchCitiesAsync("Zzz"))
                    .ReturnsAsync(new List<LocationSearchResult>());

        var result = await _controller.SearchCities("Zzz");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IList<LocationSearchResult>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Empty(response.Data!);
    }

    [Fact]
    public void Controller_HasCorrectRoutePrefix()
    {
        var routeAttr = typeof(WeatherController)
            .GetCustomAttributes(typeof(RouteAttribute), inherit: false)
            .Cast<RouteAttribute>()
            .FirstOrDefault();

        Assert.NotNull(routeAttr);
        Assert.Equal("api/weather", routeAttr.Template);
    }
}
