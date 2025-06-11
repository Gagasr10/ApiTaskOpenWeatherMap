using RestSharp;
using System;
using System.Threading.Tasks;

public class ApiClient : IApiClient
{
    private readonly RestClient _client;

    public ApiClient()
    {
        _client = new RestClient(new RestClientOptions(new Uri(ConfigHelper.BaseUrl))
        {
            Timeout = TimeSpan.FromSeconds(5),

            ThrowOnAnyError = false
        });
    }

    public async Task<RestResponse> GetWeatherData(string cityName)
    {
        var request = new RestRequest("weather");
        request.AddParameter("q", cityName);
        request.AddParameter("units", "metric");
        request.AddParameter("appid", ConfigHelper.ApiKey);

        return await _client.ExecuteAsync(request);
    }
}
