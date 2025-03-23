using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using DotNetEnv;

class Program
{
    static async Task FetchStockData()
    {
        Env.Load();
        string apiKey = Environment.GetEnvironmentVariable("API_KEY");

        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Error: API_KEY is missing or not loaded from .env file.");
            return;
        }

        string symbol = "AAPL";
        string csvFile = "stock_data.csv";
        bool fileExists = File.Exists(csvFile);

        if (!fileExists)
        {
            using StreamWriter writer = new StreamWriter(csvFile, true);
            writer.WriteLine("Timestamp,Closing Price,SMA,Trend");
        }

        while (true)
        {
            try
            {
                string url = $"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol={symbol}&interval=5min&apikey={apiKey}";

                using HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(url);
                string json = await response.Content.ReadAsStringAsync();

                JObject data = JObject.Parse(json);
                var timeSeries = data["Time Series (5min)"] as JObject;

                if (timeSeries != null && timeSeries.Count > 0)
                {
                    List<(string, double)> stockData = timeSeries.Properties()
                        .Take(10)
                        .Select(entry => (entry.Name, double.Parse(entry.Value["4. close"].ToString())))
                        .ToList();

                    double sma = stockData.Select(x => x.Item2).Average();

                    int up = stockData.Zip(stockData.Skip(1), (a, b) => b.Item2 > a.Item2 ? 1 : 0).Sum();
                    int down = stockData.Zip(stockData.Skip(1), (a, b) => b.Item2 < a.Item2 ? 1 : 0).Sum();

                    string trend = up > down ? "Uptrend" : down > up ? "Downtrend" : "Sideways";

                    using StreamWriter writer = new StreamWriter(csvFile, true);
                    foreach (var (timestamp, price) in stockData)
                    {
                        writer.WriteLine($"{timestamp},{price},{sma:F2},{trend}");
                    }

                    Console.Clear();
                    Console.WriteLine($"Stock: {symbol}");
                    Console.WriteLine($"Last 10 Closing Prices: {string.Join(", ", stockData.Select(x => x.Item2))}");
                    Console.WriteLine($"10-period SMA: {sma:F2}");
                    Console.WriteLine($"Trend: {trend}");
                    Console.WriteLine($"Data logged to {csvFile}");

                    await Task.Delay(60000);
                }
                else
                {
                    Console.WriteLine("Error: Could not retrieve valid stock data.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    static async Task Main()
    {
        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("\nProcess interrupted. Exiting gracefully...");
            Environment.Exit(0);
        };

        await FetchStockData();
    }
}
