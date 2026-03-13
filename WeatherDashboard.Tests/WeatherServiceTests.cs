using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using WeatherDashboard.API.Models;
using WeatherDashboard.API.Services;
using Xunit;

namespace WeatherDashboard.Tests;

public class WeatherServiceTests
{
    private static IOptions<ApiSettings> CreateSettings(string baseUrl = "https://api.openweathermap.org/data/2.5", string apiKey = "test-key")
        => Options.Create(new ApiSettings { BaseUrl = baseUrl, ApiKey = apiKey });

    private static IHttpClientFactory CreateFactory(HttpResponseMessage response, string clientName = "OpenWeatherMap")
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var client = new HttpClient(handler.Object) { BaseAddress = null };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(clientName)).Returns(client);
        return factory.Object;
    }

    private static string BuildCurrentWeatherJson(string cityName = "Toronto", string country = "CA",
        double temp = 22.0, double feelsLike = 24.0, int humidity = 65,
        int pressure = 1013, double windSpeedMs = 3.5,
        string description = "clear sky", string icon = "01d",
        long dt = 1710000000, double lat = 43.7, double lon = -79.4)
        => JsonSerializer.Serialize(new
        {
            name = cityName,
            dt,
            coord = new { lat, lon },
            main = new { temp, feels_like = feelsLike, humidity, pressure },
            wind = new { speed = windSpeedMs },
            weather = new[] { new { description, icon } },
            sys = new { country }
        });

    private static string BuildForecastJson(string cityName = "Toronto", string country = "CA",
        double lat = 43.7, double lon = -79.4)
    {
        var now = DateTime.UtcNow;
        var todayStr = now.ToString("yyyy-MM-dd HH:00:00");
        var day1Str = now.AddDays(1).Date.ToString("yyyy-MM-dd") + " 12:00:00";
        var day2Str = now.AddDays(2).Date.ToString("yyyy-MM-dd") + " 12:00:00";

        return JsonSerializer.Serialize(new
        {
            city = new { name = cityName, country, coord = new { lat, lon } },
            list = new[]
            {
                new { dt_txt = todayStr, main = new { temp = 20.0 }, wind = new { speed = 3.0 }, pop = 0.1, weather = new[] { new { description = "cloudy", icon = "02d" } } },
                new { dt_txt = day1Str, main = new { temp = 25.0 }, wind = new { speed = 4.0 }, pop = 0.2, weather = new[] { new { description = "sunny", icon = "01d" } } },
                new { dt_txt = day2Str, main = new { temp = 18.0 }, wind = new { speed = 2.0 }, pop = 0.5, weather = new[] { new { description = "rainy", icon = "10d" } } }
            }
        });
    }

    private static string BuildGeoJson()
        => JsonSerializer.Serialize(new[]
        {
            new { name = "Toronto", state = "Ontario", country = "CA", lat = 43.7, lon = -79.4 },
            new { name = "Toronto", state = "Ohio", country = "US", lat = 41.5, lon = -83.5 }
        });

    // ── GetCurrentWeatherAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentWeatherAsync_ValidCity_ReturnsWeatherResponse()
    {
        var json = BuildCurrentWeatherJson();
        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var service = new WeatherService(factory, CreateSettings(), NullLogger<WeatherService>.Instance);
        var result = await service.GetCurrentWeatherAsync("Toronto");

        Assert.Equal("Toronto", result.Location.City);
        Assert.Equal("CA", result.Location.Country);
        Assert.Equal(22.0, result.Current.Temperature.Celsius);
        Assert.Equal(65, result.Current.Humidity);
        Assert.Equal(1013, result.Current.Pressure);
        Assert.Equal("clear sky", result.Current.Description);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_WindSpeedConvertedToKph()
    {
        // 3.5 m/s * 3.6 = 12.6 km/h
        var json = BuildCurrentWeatherJson(windSpeedMs: 3.5);
        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var service = new WeatherService(factory, CreateSettings(), NullLogger<WeatherService>.Instance);
        var result = await service.GetCurrentWeatherAsync("Toronto");

        Assert.Equal(12.6, result.Current.WindSpeed);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_TemperatureConvertedToFahrenheit()
    {
        // 22°C → 71.6°F
        var json = BuildCurrentWeatherJson(temp: 22.0);
        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var service = new WeatherService(factory, CreateSettings(), NullLogger<WeatherService>.Instance);
        var result = await service.GetCurrentWeatherAsync("Toronto");

        Assert.Equal(71.6, result.Current.Temperature.Fahrenheit);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_CityNotFound_ThrowsKeyNotFoundException()
    {
        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = new WeatherService(factory, CreateSettings(), NullLogger<WeatherService>.Instance);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.GetCurrentWeatherAsync("UnknownCity"));
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ServerError_ThrowsHttpRequestException()
    {
        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = new WeatherService(factory, CreateSettings(), NullLogger<WeatherService>.Instance);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.GetCurrentWeatherAsync("Toronto"));
    }

    // ── GetForecastAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetForecastAsync_ValidCity_ReturnsForecastResponse()
    {
        var json = BuildForecastJson();
        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var service = new WeatherService(factory, CreateSettings(), NullLogger<WeatherService>.Instance);
        var result = await service.GetForecastAsync("Toronto");

        Assert.Equal("Toronto", result.Location.City);
        Assert.NotNull(result.TodayHourly);
        Assert.NotNull(result.ExtendedDaily);
    }

    [Fact]
    public async Task GetForecastAsync_CityNotFound_ThrowsKeyNotFoundException()
    {
        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = new WeatherService(factory, CreateSettings(), NullLogger<WeatherService>.Instance);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.GetForecastAsync("UnknownCity"));
    }

    [Fact]
    public async Task GetForecastAsync_ExtendedDailyLimitedToFive()
    {
        // Build a forecast with 6 future days
        var now = DateTime.UtcNow;
        var listItems = Enumerable.Range(1, 6).Select(i =>
            new
            {
                dt_txt = now.AddDays(i).Date.ToString("yyyy-MM-dd") + " 12:00:00",
                main = new { temp = 20.0 },
                wind = new { speed = 2.0 },
                pop = 0.1,
                weather = new[] { new { description = "sunny", icon = "01d" } }
            }).ToArray();

        var json = JsonSerializer.Serialize(new
        {
            city = new { name = "Toronto", country = "CA", coord = new { lat = 43.7, lon = -79.4 } },
            list = listItems
        });

        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var service = new WeatherService(factory, CreateSettings(), NullLogger<WeatherService>.Instance);
        var result = await service.GetForecastAsync("Toronto");

        Assert.True(result.ExtendedDaily.Count <= 5);
    }

    // ── SearchCitiesAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task SearchCitiesAsync_ValidQuery_ReturnsResults()
    {
        var json = BuildGeoJson();
        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var service = new WeatherService(factory, CreateSettings(), NullLogger<WeatherService>.Instance);
        var results = await service.SearchCitiesAsync("Tor");

        Assert.Equal(2, results.Count);
        Assert.Equal("Toronto", results[0].CityName);
        Assert.Equal("Ontario", results[0].State);
        Assert.Equal("CA", results[0].Country);
        Assert.Equal("Toronto, Ontario, CA", results[0].DisplayName);
        Assert.Equal(43.7, results[0].Coordinates.Lat);
    }

    [Fact]
    public async Task SearchCitiesAsync_EmptyArray_ReturnsEmptyList()
    {
        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        var service = new WeatherService(factory, CreateSettings(), NullLogger<WeatherService>.Instance);
        var results = await service.SearchCitiesAsync("Zzz");

        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchCitiesAsync_CityWithNoState_DisplayNameSkipsState()
    {
        var json = JsonSerializer.Serialize(new[]
        {
            new { name = "Paris", state = (string?)null, country = "FR", lat = 48.85, lon = 2.35 }
        });

        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var service = new WeatherService(factory, CreateSettings(), NullLogger<WeatherService>.Instance);
        var results = await service.SearchCitiesAsync("Par");

        Assert.Single(results);
        Assert.Equal("Paris, FR", results[0].DisplayName);
    }

    [Fact]
    public async Task SearchCitiesAsync_ServerError_ThrowsHttpRequestException()
    {
        var factory = CreateFactory(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = new WeatherService(factory, CreateSettings(), NullLogger<WeatherService>.Instance);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.SearchCitiesAsync("Tor"));
    }
}
