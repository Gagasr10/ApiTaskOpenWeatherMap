using NUnit.Framework;
using RestSharp;
using FluentAssertions;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System;
using Tests.Helpers;


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

        [Test]  // this is a kind of Smoke test 
        public async Task GetWeatherData_ValidCity_ReturnsSuccess()
        {
            Assert.That(_apiClient, Is.Not.Null);

            var response = await _apiClient!.GetWeatherData("Belgrade");

            await TestHelpers.LogResponse(
                response.StatusCode,
                response.Content ?? "<null>",
                nameof(GetWeatherData_ValidCity_ReturnsSuccess));

            response.IsSuccessful.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().NotBeNullOrEmpty();

            var weather = JsonSerializer.Deserialize<WeatherResponse>(response.Content!);
            weather.Should().NotBeNull();
            weather!.Main.Temp.Should().BeGreaterThan(-100);
            weather.Weather.Should().NotBeNull();
            weather.Weather!.Length.Should().BeGreaterThan(0);
        }


        [Test]
        public async Task MissingApiKey_ShouldReturnUnauthorized()
        {
            var client = new RestClient(new RestClientOptions(ConfigHelper.BaseUrl));
            var request = new RestRequest("weather", Method.Get);

            request.AddParameter("q", "Belgrade", ParameterType.QueryString);
            request.AddParameter("appid", "", ParameterType.QueryString);

            var response = await client.ExecuteAsync(request);

            await TestHelpers.LogResponse(
                response.StatusCode,
                response.Content ?? "<null>",
                nameof(MissingApiKey_ShouldReturnUnauthorized));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            response.Content.Should().Contain("Invalid API key");
        }



        [Test]
        public async Task InvalidApiKey_ShouldReturnUnauthorized()
        {
            var client = new RestClient(new RestClientOptions(ConfigHelper.BaseUrl));
            var request = new RestRequest("weather", Method.Get);

            request.AddParameter("q", "Belgrade", ParameterType.QueryString);
            request.AddParameter("appid", "12345", ParameterType.QueryString);

            var response = await client.ExecuteAsync(request);

            await TestHelpers.LogResponse(response.StatusCode, response.Content ?? "<null>", nameof(InvalidApiKey_ShouldReturnUnauthorized));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }


        [TestCase("metric")]
        [TestCase("imperial")]
        [TestCase("standard")]
        public async Task UnitsParameter_ShouldAffectTemperatureScale(string units)
        {
            var client = new RestClient(new RestClientOptions(ConfigHelper.BaseUrl));
            var request = new RestRequest("weather", Method.Get);

            request.AddParameter("q", "Belgrade", ParameterType.QueryString);
            request.AddParameter("appid", ConfigHelper.ApiKey, ParameterType.QueryString);
            request.AddParameter("units", units, ParameterType.QueryString);

            var response = await client.ExecuteAsync(request);

            await TestHelpers.LogResponse(response.StatusCode, response.Content ?? "<null>", $"Units_{units}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var weather = JsonSerializer.Deserialize<WeatherResponse>(response.Content!);
            weather!.Main.Temp.Should().BeGreaterThan(-100);
        }


        [Test]
        public async Task CoordinatesQuery_ShouldReturnNoviSad()
        {
            var client = new RestClient(new RestClientOptions(ConfigHelper.BaseUrl));
            var request = new RestRequest("weather", Method.Get);

            request.AddParameter("lat", "45.2517", ParameterType.QueryString);
            request.AddParameter("lon", "19.8369", ParameterType.QueryString);
            request.AddParameter("appid", ConfigHelper.ApiKey, ParameterType.QueryString);

            var response = await client.ExecuteAsync(request);

            await TestHelpers.LogResponse(response.StatusCode, response.Content ?? "<null>", nameof(CoordinatesQuery_ShouldReturnNoviSad));

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var weather = JsonSerializer.Deserialize<WeatherResponse>(response.Content!);

            weather!.Name.Should().Contain("Novi Sad");
            weather.Sys.Country.Should().Be("RS");
        }


        [Test]
        public async Task NoParameters_ShouldReturnError()
        {
            var client = new RestClient(new RestClientOptions(ConfigHelper.BaseUrl));
            var request = new RestRequest("weather", Method.Get);

            var response = await client.ExecuteAsync(request);

            await TestHelpers.LogResponse(response.StatusCode, response.Content ?? "<null>", nameof(NoParameters_ShouldReturnError));

            response.StatusCode.Should().Match(code =>
                code == HttpStatusCode.Unauthorized || code == HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task SpecialCharacters_ShouldReturnNotFound()
        {
            var response = await _apiClient!.GetWeatherData("@@@###");

            await TestHelpers.LogResponse(
                response.StatusCode,
                response.Content ?? "<null>",
                nameof(SpecialCharacters_ShouldReturnNotFound));

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task ContentType_ShouldBeApplicationJson()
        {
            var response = await _apiClient!.GetWeatherData("Belgrade");

            await TestHelpers.LogResponse(
                response.StatusCode,
                response.Content ?? "<null>",
                nameof(ContentType_ShouldBeApplicationJson));

            response.ContentType.Should().Contain("application/json");
        }


        [Test]
        public async Task ResponseTime_ShouldBeAcceptable()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _apiClient!.GetWeatherData("Belgrade");
            watch.Stop();

            var logContent = $"Response time: {watch.ElapsedMilliseconds} ms\n" +
                             $"Status: {(int)response.StatusCode} {response.StatusCode}\n" +
                             $"Response:\n{response.Content ?? "<null>"}";

            await TestHelpers.LogResponse(
                response.StatusCode,
                logContent,
                nameof(ResponseTime_ShouldBeAcceptable));

            watch.ElapsedMilliseconds.Should().BeLessThan(1500);
        }


        [Test]
        public async Task SqlInjectionLikeCityName_ShouldReturnNotFoundOrBadRequest()
        {
            var maliciousInput = "Belgrade;DROP TABLE users";
            var response = await _apiClient!.GetWeatherData(maliciousInput);

            await TestHelpers.LogResponse(
                response.StatusCode,
                response.Content ?? "<null>",
                nameof(SqlInjectionLikeCityName_ShouldReturnNotFoundOrBadRequest));

            response.StatusCode.Should().Match(status =>
                status == HttpStatusCode.NotFound || status == HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task XssInjectionLikeInput_ShouldReturnNotFoundOrBadRequest()
        {
            var maliciousInput = "<script>alert(1)</script>";
            var response = await _apiClient!.GetWeatherData(maliciousInput);

            await TestHelpers.LogResponse(
                response.StatusCode,
                response.Content ?? "<null>",
                nameof(XssInjectionLikeInput_ShouldReturnNotFoundOrBadRequest));

            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }


        [Test]
        public async Task ExcessivelyLongCityName_ShouldNotCauseServerError()
        {
            var longCity = new string('A', 500); // "AAAAA...."
            var response = await _apiClient!.GetWeatherData(longCity);

            await TestHelpers.LogResponse(
                response.StatusCode,
                response.Content ?? "<null>",
                nameof(ExcessivelyLongCityName_ShouldNotCauseServerError));

            response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task SlightlyAlteredApiKey_ShouldReturnUnauthorized()
        {
            var fakeKey = ConfigHelper.ApiKey + "123";
            var client = new RestClient(new RestClientOptions(ConfigHelper.BaseUrl));
            var request = new RestRequest("weather", Method.Get);
            request.AddParameter("q", "Belgrade");
            request.AddParameter("appid", fakeKey);

            var response = await client.ExecuteAsync(request);

            await TestHelpers.LogResponse(response.StatusCode, response.Content ?? "<null>", nameof(SlightlyAlteredApiKey_ShouldReturnUnauthorized));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }


    }
}
