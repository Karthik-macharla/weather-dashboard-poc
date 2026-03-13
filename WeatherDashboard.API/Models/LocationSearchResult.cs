namespace WeatherDashboard.API.Models;

/// <summary>
/// City search autocomplete result.
/// </summary>
public class LocationSearchResult
{
    /// <summary>Gets or sets the formatted display name (e.g. "Toronto, Ontario, Canada").</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the city name.</summary>
    public string CityName { get; set; } = string.Empty;

    /// <summary>Gets or sets the state or province.</summary>
    public string State { get; set; } = string.Empty;

    /// <summary>Gets or sets the country.</summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>Gets or sets the geographic coordinates.</summary>
    public Coordinates Coordinates { get; set; } = new();
}
