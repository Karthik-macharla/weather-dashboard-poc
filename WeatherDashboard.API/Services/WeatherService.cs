using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WeatherDashboard.API.Models;

namespace WeatherDashboard.API.Services;

/// <summary>
/// Implements weather data retrieval using the OpenWeatherMap API.
/// </summary>
public class WeatherService : IWeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<WeatherService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of <see cref="WeatherService"/>.
    /// </summary>
    public WeatherService(
        IHttpClientFactory httpClientFactory,
        IOptions<ApiSettings> apiSettings,
        ILogger<WeatherService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _apiSettings = apiSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<WeatherResponse> GetCurrentWeatherAsync(string cityName)
    {
        _logger.LogInformation("Fetching current weather for city: {CityName}", cityName);

        var client = _httpClientFactory.CreateClient("OpenWeatherMap");
        var url = $"{_apiSettings.BaseUrl}/weather?q={Uri.EscapeDataString(cityName)}&appid={_apiSettings.ApiKey}&units=metric";

        var httpResponse = await client.GetAsync(url);

        if (httpResponse.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("City not found: {CityName}", cityName);
            throw new KeyNotFoundException($"City '{cityName}' not found. Please check the spelling and try again.");
        }

        httpResponse.EnsureSuccessStatusCode();

        var content = await httpResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        return MapToWeatherResponse(root);
    }

    /// <inheritdoc/>
    public async Task<ForecastResponse> GetForecastAsync(string cityName)
    {
        _logger.LogInformation("Fetching forecast for city: {CityName}", cityName);

        var client = _httpClientFactory.CreateClient("OpenWeatherMap");
        var url = $"{_apiSettings.BaseUrl}/forecast?q={Uri.EscapeDataString(cityName)}&appid={_apiSettings.ApiKey}&units=metric&cnt=40";

        var httpResponse = await client.GetAsync(url);

        if (httpResponse.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("City not found: {CityName}", cityName);
            throw new KeyNotFoundException($"City '{cityName}' not found. Please check the spelling and try again.");
        }

        httpResponse.EnsureSuccessStatusCode();

        var content = await httpResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        return MapToForecastResponse(root);
    }

    /// <inheritdoc/>
    public async Task<IList<LocationSearchResult>> SearchCitiesAsync(string query)
    {
        _logger.LogInformation("Searching cities for query: {Query}", query);

        var client = _httpClientFactory.CreateClient("OpenWeatherMap");
        var geoUrl = $"https://api.openweathermap.org/geo/1.0/direct?q={Uri.EscapeDataString(query)}&limit=5&appid={_apiSettings.ApiKey}";

        var httpResponse = await client.GetAsync(geoUrl);
        httpResponse.EnsureSuccessStatusCode();

        var content = await httpResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);

        var results = new List<LocationSearchResult>();
        foreach (var item in doc.RootElement.EnumerateArray())
        {
            var cityName = item.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty;
            var state = item.TryGetProperty("state", out var stateProp) ? stateProp.GetString() ?? string.Empty : string.Empty;
            var country = item.TryGetProperty("country", out var countryProp) ? countryProp.GetString() ?? string.Empty : string.Empty;
            var lat = item.TryGetProperty("lat", out var latProp) ? latProp.GetDouble() : 0;
            var lon = item.TryGetProperty("lon", out var lonProp) ? lonProp.GetDouble() : 0;

            var displayParts = new List<string> { cityName };
            if (!string.IsNullOrEmpty(state)) displayParts.Add(state);
            if (!string.IsNullOrEmpty(country)) displayParts.Add(country);

            results.Add(new LocationSearchResult
            {
                DisplayName = string.Join(", ", displayParts),
                CityName = cityName,
                State = state,
                Country = country,
                Coordinates = new Coordinates { Lat = lat, Lon = lon }
            });
        }

        return results;
    }

    private static WeatherResponse MapToWeatherResponse(JsonElement root)
    {
        double tempCelsius = 0, feelsLikeCelsius = 0;
        int humidity = 0, pressure = 0;
        if (root.TryGetProperty("main", out var main))
        {
            tempCelsius = main.TryGetProperty("temp", out var t) ? t.GetDouble() : 0;
            feelsLikeCelsius = main.TryGetProperty("feels_like", out var fl) ? fl.GetDouble() : 0;
            humidity = main.TryGetProperty("humidity", out var h) ? h.GetInt32() : 0;
            pressure = main.TryGetProperty("pressure", out var p) ? p.GetInt32() : 0;
        }

        var windSpeed = root.TryGetProperty("wind", out var wind)
            ? (wind.TryGetProperty("speed", out var ws) ? ws.GetDouble() * 3.6 : 0) : 0;
        var description = string.Empty;
        var icon = string.Empty;
        if (root.TryGetProperty("weather", out var weatherArr) && weatherArr.GetArrayLength() > 0)
        {
            var w = weatherArr[0];
            description = w.TryGetProperty("description", out var desc) ? desc.GetString() ?? string.Empty : string.Empty;
            icon = w.TryGetProperty("icon", out var ic) ? ic.GetString() ?? string.Empty : string.Empty;
        }

        var cityName = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty;
        var country = root.TryGetProperty("sys", out var sys) && sys.TryGetProperty("country", out var c) ? c.GetString() ?? string.Empty : string.Empty;
        var lat = root.TryGetProperty("coord", out var coord) && coord.TryGetProperty("lat", out var la) ? la.GetDouble() : 0;
        var lon = root.TryGetProperty("coord", out var coord2) && coord2.TryGetProperty("lon", out var lo) ? lo.GetDouble() : 0;
        var dt = root.TryGetProperty("dt", out var dtProp) ? DateTimeOffset.FromUnixTimeSeconds(dtProp.GetInt64()).UtcDateTime : DateTime.UtcNow;

        return new WeatherResponse
        {
            Location = new LocationInfo
            {
                City = cityName,
                State = string.Empty,
                Country = country,
                Coordinates = new Coordinates { Lat = lat, Lon = lon }
            },
            Current = new CurrentConditions
            {
                Temperature = new Temperature { Celsius = Math.Round(tempCelsius, 1), Fahrenheit = Math.Round(tempCelsius * 9 / 5 + 32, 1) },
                FeelsLike = new Temperature { Celsius = Math.Round(feelsLikeCelsius, 1), Fahrenheit = Math.Round(feelsLikeCelsius * 9 / 5 + 32, 1) },
                Humidity = humidity,
                WindSpeed = Math.Round(windSpeed, 1),
                Pressure = pressure,
                Description = description,
                Icon = icon,
                Timestamp = dt
            }
        };
    }

    private static ForecastResponse MapToForecastResponse(JsonElement root)
    {
        var cityName = string.Empty;
        var country = string.Empty;
        double lat = 0, lon = 0;

        if (root.TryGetProperty("city", out var city))
        {
            cityName = city.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty;
            country = city.TryGetProperty("country", out var c) ? c.GetString() ?? string.Empty : string.Empty;
            if (city.TryGetProperty("coord", out var coord))
            {
                lat = coord.TryGetProperty("lat", out var la) ? la.GetDouble() : 0;
                lon = coord.TryGetProperty("lon", out var lo) ? lo.GetDouble() : 0;
            }
        }

        var location = new LocationInfo
        {
            City = cityName,
            Country = country,
            Coordinates = new Coordinates { Lat = lat, Lon = lon }
        };

        var todayHourly = new List<HourlyForecast>();
        var dailyMap = new Dictionary<DateOnly, DailyForecast>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (root.TryGetProperty("list", out var list))
        {
            foreach (var item in list.EnumerateArray())
            {
                var dtTxt = item.TryGetProperty("dt_txt", out var dtp) ? dtp.GetString() : null;
                if (dtTxt == null) continue;

                if (!DateTime.TryParse(dtTxt, out var forecastTime)) continue;
                var forecastDate = DateOnly.FromDateTime(forecastTime);

                var tempC = item.TryGetProperty("main", out var m) && m.TryGetProperty("temp", out var t) ? t.GetDouble() : 0;
                var windMs = item.TryGetProperty("wind", out var w) && w.TryGetProperty("speed", out var ws) ? ws.GetDouble() * 3.6 : 0;
                var pop = item.TryGetProperty("pop", out var popProp) ? (int)(popProp.GetDouble() * 100) : 0;
                var desc = string.Empty;
                var icon = string.Empty;
                if (item.TryGetProperty("weather", out var wa) && wa.GetArrayLength() > 0)
                {
                    var we = wa[0];
                    desc = we.TryGetProperty("description", out var d) ? d.GetString() ?? string.Empty : string.Empty;
                    icon = we.TryGetProperty("icon", out var ic) ? ic.GetString() ?? string.Empty : string.Empty;
                }

                if (forecastDate == today)
                {
                    todayHourly.Add(new HourlyForecast
                    {
                        Time = forecastTime,
                        Temperature = new Temperature { Celsius = Math.Round(tempC, 1), Fahrenheit = Math.Round(tempC * 9 / 5 + 32, 1) },
                        PrecipitationChance = pop,
                        WindSpeed = Math.Round(windMs, 1),
                        Description = desc
                    });
                }
                else if (forecastDate > today)
                {
                    if (!dailyMap.TryGetValue(forecastDate, out var daily))
                    {
                        daily = new DailyForecast
                        {
                            Date = forecastDate,
                            High = new Temperature { Celsius = tempC, Fahrenheit = tempC * 9 / 5 + 32 },
                            Low = new Temperature { Celsius = tempC, Fahrenheit = tempC * 9 / 5 + 32 },
                            Description = desc,
                            Icon = icon
                        };
                        dailyMap[forecastDate] = daily;
                    }
                    else
                    {
                        if (tempC > daily.High.Celsius)
                        {
                            daily.High = new Temperature { Celsius = Math.Round(tempC, 1), Fahrenheit = Math.Round(tempC * 9 / 5 + 32, 1) };
                        }

                        if (tempC < daily.Low.Celsius)
                        {
                            daily.Low = new Temperature { Celsius = Math.Round(tempC, 1), Fahrenheit = Math.Round(tempC * 9 / 5 + 32, 1) };
                        }
                    }

                    if (pop > daily.PrecipitationChance) daily.PrecipitationChance = pop;
                }
            }
        }

        return new ForecastResponse
        {
            Location = location,
            TodayHourly = todayHourly,
            ExtendedDaily = dailyMap.Values.OrderBy(d => d.Date).Take(5).ToList()
        };
    }
}
