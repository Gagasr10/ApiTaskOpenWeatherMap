using RestSharp;
using System.Threading.Tasks;

public interface IApiClient
{
    Task<RestResponse> GetWeatherData(string cityName);
}
