using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using WeatherDashboard.API.Middleware;
using WeatherDashboard.API.Models;
using Xunit;

namespace WeatherDashboard.Tests;

public class ExceptionHandlingMiddlewareTests
{
    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ApiResponse<object>?> ReadResponseAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return JsonSerializer.Deserialize<ApiResponse<object>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNextAndDoesNotAlterResponse()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => Task.CompletedTask,
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_KeyNotFoundException_Returns404WithMessage()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new KeyNotFoundException("City 'Xyz' not found."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
        var response = await ReadResponseAsync(context);
        Assert.NotNull(response);
        Assert.False(response!.Success);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvokeAsync_ArgumentException_Returns400()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new ArgumentException("Invalid argument."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
        var response = await ReadResponseAsync(context);
        Assert.NotNull(response);
        Assert.False(response!.Success);
    }

    [Fact]
    public async Task InvokeAsync_HttpRequestException_Returns502()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new HttpRequestException("External API error."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.BadGateway, context.Response.StatusCode);
        var response = await ReadResponseAsync(context);
        Assert.NotNull(response);
        Assert.False(response!.Success);
        Assert.Contains("weather service", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_Returns500WithGenericMessage()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("Something went wrong."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        var response = await ReadResponseAsync(context);
        Assert.NotNull(response);
        Assert.False(response!.Success);
        // Generic message — no internal details exposed
        Assert.DoesNotContain("Something went wrong", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_ContentTypeIsJson()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new Exception("Boom"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal("application/json", context.Response.ContentType);
    }
}
