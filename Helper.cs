using Microsoft.Data.SqlClient;
using System.Text.Json.Serialization;
using System.Data;
using System.Net.Http;


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
            using var tx = connection.BeginTransaction();

            // First, ensure the stock exists in the Stocks table
            int stockId = await EnsureStockExists(connection, tx, stockCode, friendlyName, chartResult.Meta);

            // Prepare data table for bulk insert
            var table = new DataTable();
            table.Columns.Add("StockId", typeof(int));
            table.Columns.Add("Timestamp", typeof(DateTime));
            table.Columns.Add("Open", typeof(decimal));
            table.Columns.Add("High", typeof(decimal));
            table.Columns.Add("Low", typeof(decimal));
            table.Columns.Add("Close", typeof(decimal));
            table.Columns.Add("Volume", typeof(long));

            if (chartResult.Timestamp != null && chartResult.Indicators?.Quote?.Count > 0)
            {
                var quote = chartResult.Indicators.Quote[0];

                for (int i = 0; i < chartResult.Timestamp.Count; i++)
                {
                    var timestampUtc = DateTimeOffset.FromUnixTimeSeconds(chartResult.Timestamp[i]).UtcDateTime;

                    var open = i < quote.Open.Count ? quote.Open[i] : null;
                    var high = i < quote.High.Count ? quote.High[i] : null;
                    var low = i < quote.Low.Count ? quote.Low[i] : null;
                    var close = i < quote.Close.Count ? quote.Close[i] : null;
                    var volume = i < quote.Volume.Count ? quote.Volume[i] : null;

                    if (open == null && high == null && low == null && close == null && volume == null)
                    {
                        continue; // skip empty row
                    }

                    var row = table.NewRow();
                    row["StockId"] = stockId;
                    row["Timestamp"] = timestampUtc;
                    row["Open"] = (object?)open ?? DBNull.Value;
                    row["High"] = (object?)high ?? DBNull.Value;
                    row["Low"] = (object?)low ?? DBNull.Value;
                    row["Close"] = (object?)close ?? DBNull.Value;
                    row["Volume"] = (object?)volume ?? DBNull.Value;
                    table.Rows.Add(row);
                }
            }

            if (table.Rows.Count > 0)
            {
                // stage into temp table
                using (var createCmd = new SqlCommand(@"CREATE TABLE #TmpStockValues(
                        StockId INT NOT NULL,
                        Timestamp DATETIME2(7) NOT NULL,
                        [Open] DECIMAL(18,4) NULL,
                        High DECIMAL(18,4) NULL,
                        Low DECIMAL(18,4) NULL,
                        [Close] DECIMAL(18,4) NULL,
                        Volume BIGINT NULL
                    )", connection, tx))
                {
                    await createCmd.ExecuteNonQueryAsync();
                }

                using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, tx))
                {
                    bulk.DestinationTableName = "#TmpStockValues";
                    await bulk.WriteToServerAsync(table);
                }

                // merge into target
                using (var mergeCmd = new SqlCommand(@"
                    MERGE dbo.StockValues AS target
                    USING #TmpStockValues AS source
                    ON target.StockId = source.StockId AND target.Timestamp = source.Timestamp
                    WHEN MATCHED THEN
                        UPDATE SET [Open] = source.[Open], High = source.High, Low = source.Low, [Close] = source.[Close], Volume = source.Volume
                    WHEN NOT MATCHED THEN
                        INSERT (StockId, Timestamp, [Open], High, Low, [Close], Volume)
                        VALUES (source.StockId, source.Timestamp, source.[Open], source.High, source.Low, source.[Close], source.Volume);", connection, tx))
                {
                    await mergeCmd.ExecuteNonQueryAsync();
                }
            }

            await tx.CommitAsync();
            Console.WriteLine($"Saved {table.Rows.Count} data points for {stockCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving data for {stockCode}: {ex.Message}");
        }
    }

    private static async Task<int> EnsureStockExists(SqlConnection connection, SqlTransaction tx, string stockCode, string friendlyName, Meta meta)
    {
        string selectQuery = "SELECT Id FROM dbo.Stocks WHERE StockCode = @StockCode";
        using var selectCommand = new SqlCommand(selectQuery, connection, tx);
        selectCommand.Parameters.AddWithValue("@StockCode", stockCode);

        var result = await selectCommand.ExecuteScalarAsync();
        if (result != null && result is int id)
        {
            return id;
        }

        // Insert new stock
        string insertQuery = @"
            INSERT INTO dbo.Stocks (StockCode, FriendlyName, Currency, ExchangeName, UpdatedDate)
            OUTPUT INSERTED.Id
            VALUES (@StockCode, @FriendlyName, @Currency, @ExchangeName, GETUTCDATE())";

        using var insertCommand = new SqlCommand(insertQuery, connection, tx);
        insertCommand.Parameters.AddWithValue("@StockCode", stockCode);
        insertCommand.Parameters.AddWithValue("@FriendlyName", (object?)friendlyName ?? DBNull.Value);
        insertCommand.Parameters.AddWithValue("@Currency", (object?)meta.Currency ?? DBNull.Value);
        insertCommand.Parameters.AddWithValue("@ExchangeName", (object?)meta.ExchangeName ?? DBNull.Value);

        var insertedResult = await insertCommand.ExecuteScalarAsync();
        if (insertedResult != null && insertedResult is int insertedId)
        {
            return insertedId;
        }
        throw new InvalidOperationException("Failed to insert or retrieve Stock Id.");
    }

    public static async Task<StockValue?>GetLatestStockDataFromDatabase(string ticker, string valuesConnectionString)
    {
        // Find newest data in the database for this stock
        using var connection = new SqlConnection(valuesConnectionString);
        await connection.OpenAsync();
        string query = @"
            SELECT TOP 1 *
            FROM dbo.StockValues
            WHERE StockId = (SELECT Id FROM dbo.Stocks WHERE StockCode = @StockCode)
            ORDER BY Timestamp DESC";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@StockCode", ticker);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new StockValue
            {
                StockId = reader.GetInt32(reader.GetOrdinal("StockId")),
                Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp")),
                Open = reader.IsDBNull(reader.GetOrdinal("Open")) ? null : reader.GetDecimal(reader.GetOrdinal("Open")),
                High = reader.IsDBNull(reader.GetOrdinal("High")) ? null : reader.GetDecimal(reader.GetOrdinal("High")),
                Low = reader.IsDBNull(reader.GetOrdinal("Low")) ? null : reader.GetDecimal(reader.GetOrdinal("Low")),
                Close = reader.IsDBNull(reader.GetOrdinal("Close")) ? null : reader.GetDecimal(reader.GetOrdinal("Close")),
                Volume = reader.IsDBNull(reader.GetOrdinal("Volume")) ? null : reader.GetInt64(reader.GetOrdinal("Volume"))
            };
        }
        return null;
    }

    public static async Task<HttpResponseMessage?> PerformHttpRequestWithRetry(HttpClient httpClient, string url, int maxAttempts = 3)
    {
        HttpResponseMessage? response = null;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    break;
                }
                Console.WriteLine($"Failed to load data (attempt {attempt}/{maxAttempts}): {response.StatusCode}");
            }
            catch (HttpRequestException hre)
            {
                Console.WriteLine($"HTTP error (attempt {attempt}/{maxAttempts}): {hre.Message}");
            }

            if (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt * attempt));
            }
        }

        if (response == null || !response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to load data: {response?.StatusCode}");
            response?.Dispose();
        }

        return response;
    }
}

