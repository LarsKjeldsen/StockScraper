using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Net.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

// Build configuration from appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Get connection string from configuration
string connectionString = configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");

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
List<Stock> stocks = await Helper.GetStocksFromDatabase(connectionString);
Console.WriteLine($"Found {stocks.Count} stocks in database");

// Process each stock
foreach (var stock in stocks)
{
    // only process ^OMXC25
    if (stock.StockCode.Trim() != "^OMXC25")
    {
        ; // continue;
    }
    try
    {
        Console.WriteLine($"\n--- Processing {stock.StockCode} ({stock.FriendlyName}) ---");
        await ProcessStock(stock.StockCode.Trim(), stock.FriendlyName, httpClient, jsonOptions, connectionString);

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
static async Task ProcessStock(string ticker, string friendlyName, HttpClient httpClient, JsonSerializerOptions jsonOptions, string valuesConnectionString)
{
    string url = $"https://query1.finance.yahoo.com/v8/finance/chart/{ticker}?interval=1m&range=8d";
    
    var response = await httpClient.GetAsync(url);
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to load data for {ticker}: {response.StatusCode}");
        return;
    }

    var jsonContent = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"Raw JSON response length: {jsonContent.Length}");

    // Parse the JSON response using our data model
    var data = JsonSerializer.Deserialize<YahooFinanceResponse>(jsonContent, jsonOptions);

    if (data == null || data.Chart == null || data.Chart.Result == null || data.Chart.Result.Count == 0)
    {
        Console.WriteLine($"No data found for {ticker}");
        return;
    }
    
    var result = data.Chart.Result[0];
    Console.WriteLine($"Stock: {ticker}");
    Console.WriteLine($"Current Price: {result.Meta.RegularMarketPrice}");
    Console.WriteLine($"Previous Close: {result.Meta.PreviousClose}");

    // Get the latest quote data for Open, High, Low
    if (result.Indicators?.Quote?.Count > 0)
    {
        var quote = result.Indicators.Quote[0];
        var latestIndex = quote.Open.Count - 1;
        
        if (latestIndex >= 0)
        {
            Console.WriteLine($"Open: {quote.Open[latestIndex]}");
            Console.WriteLine($"High: {quote.High[latestIndex]}");
            Console.WriteLine($"Low: {quote.Low[latestIndex]}");
        }
    }

    // Save to database
    await Helper.SaveStockDataToDatabase(ticker, friendlyName, result, valuesConnectionString);
}
