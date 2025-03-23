using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using DotNetEnv;

class Program
{
    static async Task Main()
    {
        Env.Load();
        string apiKey = Environment.GetEnvironmentVariable("API_KEY");

        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Error: API_KEY is missing or not loaded from .env file.");
            return;
        }

        string symbol = "AAPL";
        string url = $"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol={symbol}&interval=5min&apikey={apiKey}";

        using HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);
        string json = await response.Content.ReadAsStringAsync();

        JObject data = JObject.Parse(json);
        var timeSeries = data["Time Series (5min)"] as JObject;

        if (timeSeries != null && timeSeries.Count > 0)
        {
            List<double> closingPrices = timeSeries.Properties()
                .Take(10)
                .Select(entry => double.Parse(entry.Value["4. close"].ToString()))
                .ToList();

            double sma = closingPrices.Average();
            
            Console.WriteLine($"Stock: {symbol}");
            Console.WriteLine($"Last 10 Closing Prices: {string.Join(", ", closingPrices)}");
            Console.WriteLine($"10-period Simple Moving Average (SMA): {sma:F2}");
        }
        else
        {
            Console.WriteLine("Error: Could not retrieve valid stock data.");
        }
    }
}
