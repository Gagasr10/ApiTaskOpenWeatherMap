using NUnit.Framework;
using FluentAssertions;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System;

namespace Tests
{
    public class WeatherApiTests
    {
        private IApiClient? _apiClient;

        [SetUp]
        public void Setup()
        {
            string rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            string envPath = Path.Combine(rootPath, ".env");

            DotNetEnv.Env.Load(envPath);

            if (string.IsNullOrWhiteSpace(ConfigHelper.ApiKey))
            {
                Assert.Ignore("API key not set; skipping tests.");
            }

            _apiClient = new ApiClient();
        }

        [Test]
        public async Task GetWeatherData_ValidCity_ReturnsSuccess()
        {
            Assert.That(_apiClient, Is.Not.Null);

            var response = await _apiClient!.GetWeatherData("Belgrade");

            TestContext.WriteLine("Status: " + response.StatusCode);
            TestContext.WriteLine("RAW RESPONSE:");
            TestContext.WriteLine(response.Content ?? "<null>");

            if (string.IsNullOrWhiteSpace(response.Content))
            {
                Assert.Fail($"Empty response content. Status: {response.StatusCode}");
                return;
            }

            WeatherResponse? weather;
            try
            {
                weather = JsonSerializer.Deserialize<WeatherResponse>(response.Content);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to parse JSON. Error: {ex.Message}\nResponse: {response.Content}");
                return;
            }

            if (weather?.Main == null)
            {
                Assert.Fail("'main' section missing in API response.\nResponse:\n" + response.Content);
                return;
            }

            string projectRoot = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
            string logDir = Path.Combine(projectRoot, "Logs");
            Directory.CreateDirectory(logDir);
            string logPath = Path.Combine(logDir, "log.txt");

            await File.AppendAllTextAsync(logPath,
                $"[{DateTime.Now}] Status: {(int)response.StatusCode} {response.StatusCode}\n" +
                $"City: Belgrade\n" +
                $"Temperature: {weather.Main.Temp}\n" +
                $"RAW RESPONSE:\n{response.Content}\n\n");

            response.IsSuccessful.Should().BeTrue($"Expected success, got: {response.StatusCode}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().NotBeNullOrEmpty("Response content should not be empty");

            weather.Main.Temp.Should().BeGreaterThan(-100, "Temperature should be valid");
            weather.Weather.Should().NotBeNull("Weather array missing");
            weather.Weather!.Length.Should().BeGreaterThan(0, "Weather array is empty");

            TestContext.WriteLine("Parsed temperature: " + weather.Main.Temp);
        }
    }
}
