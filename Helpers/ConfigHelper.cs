using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using DotNetEnv;



public static class ConfigHelper
{
    private static readonly IConfigurationRoot Config;

    static ConfigHelper()
    {
        // Učitava .env fajl iz root direktorijuma
        DotNetEnv.Env.Load();

        Config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables()
            .Build();

        var configSection = Config.GetSection("OpenWeatherMap");
        if (!configSection.Exists())
        {
            throw new InvalidOperationException("OpenWeatherMap section missing in configuration.");
        }
    }

    public static string ApiKey => Config["OpenWeatherMap:ApiKey"]
        ?? throw new InvalidOperationException("OpenWeatherMap:ApiKey not configured.");

    public static string BaseUrl => Config["OpenWeatherMap:BaseUrl"]
        ?? throw new InvalidOperationException("OpenWeatherMap:BaseUrl not configured.");
}
