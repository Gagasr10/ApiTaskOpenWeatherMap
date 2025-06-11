using System;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var client = new ApiClient();
        var response = await client.GetWeatherData("Belgrade");

        Console.WriteLine(response.Content);
    }
}
