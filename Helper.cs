using Microsoft.Data.SqlClient;
using System.Text.Json.Serialization;


// Stock model for database
public class Stock
{
    public string StockCode { get; set; } = string.Empty;
    public string FriendlyName { get; set; } = string.Empty;
}

// Helper class for stock values
public class StockValue
{
    public int StockId { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal? Open { get; set; }
    public decimal? High { get; set; }
    public decimal? Low { get; set; }
    public decimal? Close { get; set; }
    public long? Volume { get; set; }
}

// Yahoo Finance response models
public class YahooFinanceResponse
{
    [JsonPropertyName("chart")]
    public Chart Chart { get; set; } = new();
}

public class Chart
{
    [JsonPropertyName("result")]
    public List<ChartResult> Result { get; set; } = new();

    [JsonPropertyName("error")]
    public object? Error { get; set; }
}

public class ChartResult
{
    [JsonPropertyName("meta")]
    public Meta Meta { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public List<long> Timestamp { get; set; } = new();

    [JsonPropertyName("indicators")]
    public Indicators Indicators { get; set; } = new();
}

public class Meta
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("exchangeName")]
    public string ExchangeName { get; set; } = string.Empty;

    [JsonPropertyName("fullExchangeName")]
    public string FullExchangeName { get; set; } = string.Empty;

    [JsonPropertyName("instrumentType")]
    public string InstrumentType { get; set; } = string.Empty;

    [JsonPropertyName("firstTradeDate")]
    public long FirstTradeDate { get; set; }

    [JsonPropertyName("regularMarketTime")]
    public long RegularMarketTime { get; set; }

    [JsonPropertyName("hasPrePostMarketData")]
    public bool HasPrePostMarketData { get; set; }

    [JsonPropertyName("gmtoffset")]
    public int GmtOffset { get; set; }

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = string.Empty;

    [JsonPropertyName("exchangeTimezoneName")]
    public string ExchangeTimezoneName { get; set; } = string.Empty;

    [JsonPropertyName("regularMarketPrice")]
    public decimal RegularMarketPrice { get; set; }

    [JsonPropertyName("chartPreviousClose")]
    public decimal ChartPreviousClose { get; set; }

    [JsonPropertyName("previousClose")]
    public decimal PreviousClose { get; set; }

    [JsonPropertyName("scale")]
    public int Scale { get; set; }

    [JsonPropertyName("priceHint")]
    public int PriceHint { get; set; }

    [JsonPropertyName("currentTradingPeriod")]
    public CurrentTradingPeriod CurrentTradingPeriod { get; set; } = new();

    [JsonPropertyName("tradingPeriods")]
    public List<List<TradingPeriod>> TradingPeriods { get; set; } = new();

    [JsonPropertyName("dataGranularity")]
    public string DataGranularity { get; set; } = string.Empty;

    [JsonPropertyName("range")]
    public string Range { get; set; } = string.Empty;

    [JsonPropertyName("validRanges")]
    public List<string> ValidRanges { get; set; } = new();
}

public class CurrentTradingPeriod
{
    [JsonPropertyName("pre")]
    public TradingPeriod Pre { get; set; } = new();

    [JsonPropertyName("regular")]
    public TradingPeriod Regular { get; set; } = new();

    [JsonPropertyName("post")]
    public TradingPeriod Post { get; set; } = new();
}

public class TradingPeriod
{
    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = string.Empty;

    [JsonPropertyName("start")]
    public long Start { get; set; }

    [JsonPropertyName("end")]
    public long End { get; set; }

    [JsonPropertyName("gmtoffset")]
    public int GmtOffset { get; set; }
}

public class Indicators
{
    [JsonPropertyName("quote")]
    public List<Quote> Quote { get; set; } = new();

    [JsonPropertyName("adjclose")]
    public List<AdjustedClose>? AdjustedClose { get; set; }
}

public class Quote
{
    [JsonPropertyName("open")]
    public List<decimal?> Open { get; set; } = new();

    [JsonPropertyName("close")]
    public List<decimal?> Close { get; set; } = new();

    [JsonPropertyName("high")]
    public List<decimal?> High { get; set; } = new();

    [JsonPropertyName("low")]
    public List<decimal?> Low { get; set; } = new();

    [JsonPropertyName("volume")]
    public List<long?> Volume { get; set; } = new();
}

public class AdjustedClose
{
    [JsonPropertyName("adjclose")]
    public List<decimal?> AdjClose { get; set; } = new();
}


public static class Helper
{
    public static async Task<List<Stock>> GetStocksFromDatabase(string connectionString)
    {
        var stocks = new List<Stock>();

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        string query = "SELECT StockCode, FriendlyName FROM dbo.Stocks WHERE StockCode IS NOT NULL";

        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            stocks.Add(new Stock
            {
                StockCode = reader["StockCode"]?.ToString() ?? string.Empty,
                FriendlyName = reader["FriendlyName"]?.ToString() ?? string.Empty
            });
        }

        return stocks;
    }

    public static async Task SaveStockDataToDatabase(string stockCode, string friendlyName, ChartResult chartResult, string connectionString)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // First, ensure the stock exists in the Stocks table
            int stockId = await EnsureStockExists(connection, stockCode, friendlyName, chartResult.Meta);

            // Prepare data for bulk insert
            var stockValues = new List<StockValue>();

            if (chartResult.Timestamp != null && chartResult.Indicators?.Quote?.Count > 0)
            {
                var quote = chartResult.Indicators.Quote[0];

                for (int i = 0; i < chartResult.Timestamp.Count; i++)
                {
                    var timestamp = DateTimeOffset.FromUnixTimeSeconds(chartResult.Timestamp[i]).DateTime;

                    stockValues.Add(new StockValue
                    {
                        StockId = stockId,
                        Timestamp = timestamp,
                        Open = i < quote.Open.Count ? quote.Open[i] : null,
                        High = i < quote.High.Count ? quote.High[i] : null,
                        Low = i < quote.Low.Count ? quote.Low[i] : null,
                        Close = i < quote.Close.Count ? quote.Close[i] : null,
                        Volume = i < quote.Volume.Count ? quote.Volume[i] : null
                    });
                }
            }

            // Bulk insert stock values
            await BulkInsertStockValues(connection, stockValues);

            Console.WriteLine($"Saved {stockValues.Count} data points for {stockCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving data for {stockCode}: {ex.Message}");
        }
    }

    private static async Task<int> EnsureStockExists(SqlConnection connection, string stockCode, string friendlyName, Meta meta)
    {
        string selectQuery = "SELECT Id FROM dbo.Stocks WHERE StockCode = @StockCode";
        using var selectCommand = new SqlCommand(selectQuery, connection);
        selectCommand.Parameters.AddWithValue("@StockCode", stockCode);

        var result = await selectCommand.ExecuteScalarAsync();
        if (result != null)
        {
            return (int)result;
        }

        // Insert new stock
        string insertQuery = @"
            INSERT INTO dbo.Stocks (StockCode, FriendlyName, Currency, ExchangeName, UpdatedDate)
            OUTPUT INSERTED.Id
            VALUES (@StockCode, @FriendlyName, @Currency, @ExchangeName, GETUTCDATE())";

        using var insertCommand = new SqlCommand(insertQuery, connection);
        insertCommand.Parameters.AddWithValue("@StockCode", stockCode);
        insertCommand.Parameters.AddWithValue("@FriendlyName", friendlyName ?? (object)DBNull.Value);
        insertCommand.Parameters.AddWithValue("@Currency", meta.Currency ?? (object)DBNull.Value);
        insertCommand.Parameters.AddWithValue("@ExchangeName", meta.ExchangeName ?? (object)DBNull.Value);

        return (int)await insertCommand.ExecuteScalarAsync();
    }

    private static async Task BulkInsertStockValues(SqlConnection connection, List<StockValue> stockValues)
    {
        if (stockValues.Count == 0) return;

        // For SQL Server, we need to use MERGE instead of ON DUPLICATE KEY UPDATE
        string mergeQuery = @"
            MERGE dbo.StockValues AS target
            USING (VALUES (@StockId, @Timestamp, @Open, @High, @Low, @Close, @Volume)) 
                AS source (StockId, Timestamp, [Open], High, Low, [Close], Volume)
            ON target.StockId = source.StockId AND target.Timestamp = source.Timestamp
            WHEN MATCHED THEN
                UPDATE SET [Open] = source.[Open], High = source.High, Low = source.Low, 
                          [Close] = source.[Close], Volume = source.Volume
            WHEN NOT MATCHED THEN
                INSERT (StockId, Timestamp, [Open], High, Low, [Close], Volume)
                VALUES (source.StockId, source.Timestamp, source.[Open], source.High, source.Low, source.[Close], source.Volume);";

        foreach (var stockValue in stockValues)
        {
            using var command = new SqlCommand(mergeQuery, connection);
            command.Parameters.AddWithValue("@StockId", stockValue.StockId);
            command.Parameters.AddWithValue("@Timestamp", stockValue.Timestamp);
            command.Parameters.AddWithValue("@Open", stockValue.Open ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@High", stockValue.High ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Low", stockValue.Low ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Close", stockValue.Close ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Volume", stockValue.Volume ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }
    }
}

