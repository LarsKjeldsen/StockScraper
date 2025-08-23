using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Net;
using System.Net.Http;
using StockData.Common;


// HttpClient for Yahoo Finance API calls
using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
httpClient.Timeout = TimeSpan.FromSeconds(30);

// JsonSerializerOptions to ignore case when deserializing JSON property names
var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

// Get stocks from database
List<Stock> stocks = await Helper.GetStocksFromDatabase();
Console.WriteLine($"Found {stocks.Count} stocks in database");

// Process each stock
foreach (var stock in stocks)
{

    try
    {
        // Find newest data in the database for this stock

        var latestData = await Helper.GetLatestStockDataFromDatabase(stock.StockCode);

        if ((latestData == null || latestData.Timestamp < DateTime.UtcNow.AddDays(-30)))
        {
            await ProcessStock(stock.StockCode.Trim(), stock.FriendlyName, httpClient, jsonOptions, "1h", "2y");
            await ProcessStock(stock.StockCode.Trim(), stock.FriendlyName, httpClient, jsonOptions, "5m", "1mo");
            await ProcessStock(stock.StockCode.Trim(), stock.FriendlyName, httpClient, jsonOptions, "1m", "8d");
        }
        else if (latestData.Timestamp < DateTime.UtcNow.AddDays(-1))
        {
            await ProcessStock(stock.StockCode.Trim(), stock.FriendlyName, httpClient, jsonOptions, "1m", "8d");
        }
        else
            await ProcessStock(stock.StockCode.Trim(), stock.FriendlyName, httpClient, jsonOptions, "1m", "1d");


        // Add a small delay between requests to be respectful to Yahoo Finance
        await Task.Delay(1000);
    }
    catch (Exception ex)
    {
        // Write error in red
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error processing {stock.StockCode}: {ex.Message}");
        Console.ResetColor();
    }
}

Console.WriteLine("\nProcessing complete!");


// Method to process individual stock
static async Task ProcessStock(string ticker, string friendlyName, HttpClient httpClient, JsonSerializerOptions jsonOptions, string interval, string range)
{
    string url = $"https://query1.finance.yahoo.com/v8/finance/chart/{ticker}?interval={interval}&range={range}";

    // lightweight retry
    HttpResponseMessage? response = await Helper.PerformHttpRequestWithRetry(httpClient, url, maxAttempts: 3);

    if (response == null || !response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to load data for {ticker}: {response?.StatusCode}");
        response?.Dispose();
        return;
    }

    await using var stream = await response.Content.ReadAsStreamAsync();

    // Parse the JSON response using our data model (streaming)
    var data = await JsonSerializer.DeserializeAsync<YahooFinanceResponse>(stream, jsonOptions);

    if (data?.Chart?.Result == null || data.Chart.Result.Count == 0)
    {
        Console.WriteLine($"No data found for {ticker}");
        return;
    }
    
    var result = data.Chart.Result[0];

    // Save to database
    await Helper.SaveStockDataToDatabase(ticker, friendlyName, result);
}

