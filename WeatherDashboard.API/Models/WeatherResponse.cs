namespace WeatherDashboard.API.Models;

/// <summary>
/// Geographic coordinates.
/// </summary>
public class Coordinates
{
    /// <summary>Gets or sets the latitude.</summary>
    public double Lat { get; set; }

    /// <summary>Gets or sets the longitude.</summary>
    public double Lon { get; set; }
}

/// <summary>
/// Location information for a city.
/// </summary>
public class LocationInfo
{
    /// <summary>Gets or sets the city name.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Gets or sets the state or province.</summary>
    public string State { get; set; } = string.Empty;

    /// <summary>Gets or sets the country.</summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>Gets or sets the geographic coordinates.</summary>
    public Coordinates Coordinates { get; set; } = new();
}

/// <summary>
/// Temperature in both Celsius and Fahrenheit.
/// </summary>
public class Temperature
{
    /// <summary>Gets or sets the temperature in Celsius.</summary>
    public double Celsius { get; set; }

    /// <summary>Gets or sets the temperature in Fahrenheit.</summary>
    public double Fahrenheit { get; set; }
}

/// <summary>
/// Current weather conditions.
/// </summary>
public class CurrentConditions
{
    /// <summary>Gets or sets the current temperature.</summary>
    public Temperature Temperature { get; set; } = new();

    /// <summary>Gets or sets the feels-like temperature.</summary>
    public Temperature FeelsLike { get; set; } = new();

    /// <summary>Gets or sets the humidity percentage.</summary>
    public int Humidity { get; set; }

    /// <summary>Gets or sets the wind speed in km/h.</summary>
    public double WindSpeed { get; set; }

    /// <summary>Gets or sets the atmospheric pressure in hPa.</summary>
    public int Pressure { get; set; }

    /// <summary>Gets or sets the weather description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the weather icon identifier.</summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>Gets or sets the data timestamp (UTC).</summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Full current weather response data.
/// </summary>
public class WeatherResponse
{
    /// <summary>Gets or sets the location information.</summary>
    public LocationInfo Location { get; set; } = new();

    /// <summary>Gets or sets the current weather conditions.</summary>
    public CurrentConditions Current { get; set; } = new();
}
