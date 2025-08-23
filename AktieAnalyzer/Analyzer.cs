using StockData.Common;

namespace AktieAnalyzer
{
    public class AnalysisResult
    {
        public string Recommendation { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public decimal Amount { get; set; } = 0;
        public int NumberOfTransactions { get; set; } = 0; // Added property for number of transactions
    }

    public class Analyzer
    {
        private readonly string _stockCode;
        private readonly string _friendlyName;
        private readonly List<StockValue> _stockValues;
        private readonly TimeSpan _timeRange;

        public Analyzer(string stockCode, string friendlyName, List<StockValue> stockValues, TimeSpan timeRange)
        {
            _stockCode = stockCode;
            _friendlyName = friendlyName;
            _stockValues = stockValues;
            _timeRange = timeRange;
        }

        public AnalysisResult Analyze()
        {
            if (_stockValues.Count < 10)
            {
                return new AnalysisResult
                {
                    Recommendation = "HOLD",
                    Reason = "Insufficient data for analysis"
                };
            }

            // Simple analysis based on recent price trend
            var recentValues = _stockValues.OrderByDescending(sv => sv.Timestamp).Take(10).ToList();
            var earliestPrice = recentValues.Last().Close ?? 0;
            var latestPrice = recentValues.First().Close ?? 0;

            if (earliestPrice == 0)
            {
                return new AnalysisResult
                {
                    Recommendation = "HOLD",
                    Reason = "Unable to determine price trend"
                };
            }

            var percentChange = ((latestPrice - earliestPrice) / earliestPrice) * 100;

            if (percentChange > 5)
            {
                return new AnalysisResult
                {
                    Recommendation = "BUY",
                    Reason = $"Strong upward trend: {percentChange:F2}% increase over recent period"
                };
            }
            else if (percentChange < -5)
            {
                return new AnalysisResult
                {
                    Recommendation = "SELL",
                    Reason = $"Downward trend: {percentChange:F2}% decrease over recent period"
                };
            }
            else
            {
                return new AnalysisResult
                {
                    Recommendation = "HOLD",
                    Reason = $"Stable price movement: {percentChange:F2}% change over recent period"
                };
            }
        }

        public AnalysisResult AnalyzeBuyAt8SellAt14()
        {
            // Filter stock values based on the specified time range
            var filteredStockValues = _stockValues
                .Where(sv => sv.Timestamp >= DateTime.Now.Subtract(_timeRange))
                .ToList();

            // Group stock values by date
            var groupedByDate = filteredStockValues.GroupBy(sv => sv.Timestamp.Date).ToList();

            // Starting amount in dollars
            var currentAmount = 100m;
            int transactionCount = 0; // Counter for transactions

            foreach (var group in groupedByDate)
            {
                var buyPrice = group
                    .Where(sv => sv.Timestamp.TimeOfDay >= new TimeSpan(8, 0, 0))
                    .OrderBy(sv => sv.Timestamp.TimeOfDay)
                    .FirstOrDefault()?.Close;

                var sellPrice = group
                    .Where(sv => sv.Timestamp.TimeOfDay >= new TimeSpan(14, 0, 0))
                    .OrderBy(sv => sv.Timestamp.TimeOfDay)
                    .FirstOrDefault()?.Close;

                if (buyPrice.HasValue && sellPrice.HasValue)
                {
                    // Calculate the number of shares bought and their value at sell price
                    var sharesBought = currentAmount / buyPrice.Value;
                    currentAmount = sharesBought * sellPrice.Value; // Update current amount after selling
                    transactionCount++; // Increment transaction count
                }
            }

            if (transactionCount == 0)
            {
                return new AnalysisResult
                {
                    Recommendation = "HOLD",
                    Reason = "No sufficient data for 8:00 and 14:00 analysis",
                    NumberOfTransactions = transactionCount // Include transaction count
                };
            }

            var totalProfit = currentAmount - 100m;

            return new AnalysisResult
            {
                Recommendation = totalProfit > 0 ? "Win" : "Loss",
                Reason = $"Total profit: {totalProfit:F2}, Final amount: {currentAmount:F2}",
                Amount = totalProfit, // Final profit in dollars
                NumberOfTransactions = transactionCount // Include transaction count
            };
        }
    }
}