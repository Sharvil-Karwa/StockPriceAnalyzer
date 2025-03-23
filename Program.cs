using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

class StockAnalyzer
{
    static List<(DateTime, double)> ReadStockData(string filePath)
    {
        List<(DateTime, double)> stockPrices = new List<(DateTime, double)>();

        using (StreamReader reader = new StreamReader(filePath))
        {
            string header = reader.ReadLine(); 
            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();
                if (line == null) continue;

                string[] values = line.Split(',');

                if (DateTime.TryParseExact(values[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timestamp) &&
                    double.TryParse(values[1], out double closePrice))
                {
                    stockPrices.Add((timestamp, closePrice));
                }
            }
        }

        return stockPrices.OrderBy(x => x.Item1).TakeLast(50).ToList();
    }

    static double CalculateSMA(List<(DateTime, double)> prices, int period)
    {
        if (prices.Count < period) return 0;
        return prices.TakeLast(period).Average(x => x.Item2);
    }

    static string DetectTrends(List<(DateTime, double)> prices)
    {
        if (prices.Count < 10) return "Not enough data";

        double sma10 = CalculateSMA(prices, 10);
        double sma20 = CalculateSMA(prices, 20);
        double lastPrice = prices.Last().Item2;

        string trend;
        if (sma10 > sma20) trend = "Uptrend 📈";
        else if (sma10 < sma20) trend = "Downtrend 📉";
        else trend = "Sideways Movement 🔍";

        return $"{lastPrice:F2},{sma10:F2},{sma20:F2},{trend}";
    }

    static void SaveReport(List<(DateTime, double)> prices, string reportFile)
    {
        string result = DetectTrends(prices);

        using (StreamWriter writer = new StreamWriter(reportFile, false))
        {
            writer.WriteLine("Latest Price,10-SMA,20-SMA,Trend");
            writer.WriteLine(result);
        }

        Console.WriteLine($"📊 Report saved: {reportFile}");
    }

    static void Main()
    {
        string csvFile = "stock_data.csv";
        string reportFile = "stock_report.csv";

        if (!File.Exists(csvFile))
        {
            Console.WriteLine("Error: No stock data found.");
            return;
        }

        List<(DateTime, double)> stockPrices = ReadStockData(csvFile);
        SaveReport(stockPrices, reportFile);
    }
}
