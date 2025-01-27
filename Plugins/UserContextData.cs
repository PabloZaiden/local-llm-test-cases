using System.Text.Json;
using Microsoft.SemanticKernel;

namespace LocalLlmTestCases;

class UserContextData
{
    JsonDocument GetGeoData()
    {
        using (var client = new HttpClient())
        {
            var ip = client.GetStringAsync("https://ifconfig.io/ip").Result;
            var geoDataUrl = $"http://ip-api.com/json/{ip}";
            var geoDataResponse = client.GetStringAsync(geoDataUrl).Result;

            return JsonDocument.Parse(geoDataResponse);
        }
    }

    [KernelFunction]
    public string GetCurrentLocation()
    {
        return GetGeoData().RootElement.ToString();
    }

    [KernelFunction]
    public DateTimeOffset GetCurrentDateAndTime()
    {
        return DateTimeOffset.Now;
    }

    [KernelFunction]
    public string GetPublicIP()
    {
        using (var client = new HttpClient())
        {
            return client.GetStringAsync("https://ifconfig.io/ip").Result;
        }
    }
}