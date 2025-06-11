using System.Text.Json.Serialization;

public class WeatherResponse
{
    [JsonPropertyName("main")]
    public MainInfo Main { get; set; } = default!;

    [JsonPropertyName("weather")]
    public WeatherDescription[] Weather { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("sys")]
    public SysInfo Sys { get; set; } = default!;
}

public class MainInfo
{
    [JsonPropertyName("temp")]
    public float Temp { get; set; }

    [JsonPropertyName("humidity")]
    public float Humidity { get; set; }
}

public class WeatherDescription
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class SysInfo
{
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;
}
