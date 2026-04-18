using System.Net.Http.Json;

namespace M1ndLink.Services;

public class WeatherService : IWeatherService
{
    // WMO Weather Interpretation Codes
    private static (string Condition, string Emoji) InterpretWmo(int code) => code switch
    {
        0            => ("Clear Sky",           "☀️"),
        1 or 2       => ("Partly Cloudy",       "🌤️"),
        3            => ("Overcast",             "☁️"),
        45 or 48     => ("Foggy",               "🌫️"),
        51 or 53 or 55 or 61 or 63 or 65 or 80 or 81 or 82
                     => ("Rainy",               "🌧️"),
        71 or 73 or 75 or 77 or 85 or 86
                     => ("Snowy",               "❄️"),
        95 or 96 or 99
                     => ("Thunderstorm",        "⛈️"),
        _            => ("Cloudy",              "🌥️"),
    };

    public async Task<WeatherSnapshot?> GetCurrentWeatherAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                return null;

            var location = await Geolocation.GetLastKnownLocationAsync()
                           ?? await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Low));
            if (location == null) return null;

            var lat = location.Latitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            var lon = location.Longitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            var root = await http.GetFromJsonAsync<OpenMeteoRoot>(url);
            if (root?.CurrentWeather == null) return null;

            var (condition, emoji) = InterpretWmo(root.CurrentWeather.Weathercode);
            return new WeatherSnapshot(root.CurrentWeather.Temperature, condition, emoji);
        }
        catch
        {
            return null; // graceful degradation
        }
    }

    // ── Minimal JSON models ──────────────────────────────────────────────────
    private sealed class OpenMeteoRoot
    {
        [System.Text.Json.Serialization.JsonPropertyName("current_weather")]
        public CurrentWeather? CurrentWeather { get; set; }
    }

    private sealed class CurrentWeather
    {
        [System.Text.Json.Serialization.JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("weathercode")]
        public int Weathercode { get; set; }
    }
}
