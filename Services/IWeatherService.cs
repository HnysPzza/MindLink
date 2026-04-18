namespace M1ndLink.Services;

public record WeatherSnapshot(double TemperatureCelsius, string Condition, string Emoji);

public interface IWeatherService
{
    /// <summary>Fetches current weather. Returns null if location denied or API fails.</summary>
    Task<WeatherSnapshot?> GetCurrentWeatherAsync();
}
