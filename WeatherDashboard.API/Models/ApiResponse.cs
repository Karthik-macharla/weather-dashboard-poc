namespace WeatherDashboard.API.Models;

/// <summary>
/// Standardized API response wrapper.
/// </summary>
/// <typeparam name="T">The type of data returned.</typeparam>
public class ApiResponse<T>
{
    /// <summary>Gets or sets a value indicating whether the request was successful.</summary>
    public bool Success { get; set; }

    /// <summary>Gets or sets the human-readable message describing the result.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the response data payload.</summary>
    public T? Data { get; set; }

    /// <summary>Creates a successful response.</summary>
    public static ApiResponse<T> Ok(T data, string message = "Request completed successfully")
        => new() { Success = true, Message = message, Data = data };

    /// <summary>Creates an error response.</summary>
    public static ApiResponse<T> Fail(string message)
        => new() { Success = false, Message = message, Data = default };
}
