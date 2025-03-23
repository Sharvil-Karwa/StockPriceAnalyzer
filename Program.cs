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
            reader.ReadLine();
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

        return $"{lastPrice:F2},{sma10:F2},{sma20:F2},{(sma10 > sma20 ? "Uptrend 📈" : sma10 < sma20 ? "Downtrend 📉" : "Sideways 🔍")}";
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

    static void DisplayTrend(List<(DateTime, double)> prices)
    {
        string result = DetectTrends(prices);
        Console.WriteLine("\n📈 Stock Trend Analysis:");
        Console.WriteLine("----------------------------------------------------");
        Console.WriteLine("Latest Price   | 10-day SMA   | 20-day SMA   | Trend");
        Console.WriteLine(result.Replace(",", "     | "));
        Console.WriteLine("----------------------------------------------------");
    }

    static void ShowLastEntries(List<(DateTime, double)> prices, int count)
    {
        Console.WriteLine($"\n📋 Last {count} Stock Prices:");
        foreach (var entry in prices.TakeLast(count))
        {
            Console.WriteLine($"{entry.Item1}: {entry.Item2:F2}");
        }
    }

    static void CalculateCustomSMA(List<(DateTime, double)> prices)
    {
        Console.Write("\nEnter SMA period: ");
        if (int.TryParse(Console.ReadLine(), out int period) && period > 0)
        {
            double sma = CalculateSMA(prices, period);
            Console.WriteLine($"SMA-{period}: {sma:F2}");
        }
        else
        {
            Console.WriteLine("Invalid period.");
        }
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

        while (true)
        {
            Console.WriteLine("\n📊 Stock Analyzer Menu:");
            Console.WriteLine("1. Download Report");
            Console.WriteLine("2. View Trend in Console");
            Console.WriteLine("3. Show Last 5 Entries");
            Console.WriteLine("4. Calculate Custom SMA");
            Console.WriteLine("5. Exit");
            Console.Write("Choose an option: ");

            string? choice = Console.ReadLine();
            if (choice == "1") SaveReport(stockPrices, reportFile);
            else if (choice == "2") DisplayTrend(stockPrices);
            else if (choice == "3") ShowLastEntries(stockPrices, 5);
            else if (choice == "4") CalculateCustomSMA(stockPrices);
            else if (choice == "5") break;
            else Console.WriteLine("Invalid choice. Try again.");
        }
    }
}
