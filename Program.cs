using System;
using Newtonsoft.Json;

class Program
{
    static void Main()
    {
        string json = "{ \"symbol\": \"AAPL\", \"price\": 179.23 }";
        Stock stock = JsonConvert.DeserializeObject<Stock>(json);
        Console.WriteLine($"Stock: {stock.Symbol}, Price: ${stock.Price}");
    }
}

class Stock
{
    public string Symbol { get; set; }
    public double Price { get; set; }
}
