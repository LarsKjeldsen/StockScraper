using StockData.Common;

namespace AktieAnalyzer
{
    public class AnalysisResult
    {
        public decimal Amount { get; set; } = 0;
        public int NumberOfTransactions { get; set; } = 0; // Added property for number of transactions
        public decimal TotalCommission { get; internal set; }
        public List<DailyResult> DailyResults { get; set; } = new List<DailyResult>();
    }

    public class DailyResult
    {
        public DateTime Date { get; set; }
        public decimal ProfitOrLoss { get; set; }
        public decimal currentAmount { get; set; }
        public int numStocks { get; set; }
        public decimal totalCommission { get; internal set; }
    }

    public class Analyzer
    {
        private readonly string _stockCode;
        private readonly string _friendlyName;
        private readonly List<StockValue> _stockValues;
        private readonly TimeSpan _timeRange;
        private readonly decimal _startAmount;

        public Analyzer(string stockCode, string friendlyName, List<StockValue> stockValues, TimeSpan timeRange, decimal startAmount)
        {
            _stockCode = stockCode;
            _friendlyName = friendlyName;
            _stockValues = stockValues;
            _timeRange = timeRange;
            _startAmount = startAmount;
        }


        public AnalysisResult AnalyzeBuyAt8SellAt14(bool includeCommission)
        {
            // Filter and group stock values in a single operation for better performance
            var cutoffDate = DateTime.Now.Subtract(_timeRange);
            var groupedByDate = _stockValues
                .Where(sv => sv.Timestamp >= cutoffDate)
                .GroupBy(sv => sv.Timestamp.Date)
                .OrderBy(g => g.Key)
                .ToList();

            if (!groupedByDate.Any())
            {
                return new AnalysisResult
                {
                    NumberOfTransactions = 0,
                    DailyResults = new List<DailyResult>(),
                };
            }

            // Define time spans once for reuse
            var buyTime7AM = new TimeSpan(7, 0, 0);
            var sellTime8AM = new TimeSpan(8, 0, 0);
            var buyTime2PM = new TimeSpan(14, 0, 0);
            var finalSellTime3PM = new TimeSpan(15, 0, 0);

            var currentAmount = _startAmount;
            var transactionCount = 0;
            var dailyResults = new List<DailyResult>();
            var numStocks = 0;
            decimal TotalCommission = 0;
            decimal? lastSellPrice = 0;
            var amount = 0m;

            // Initial stock purchase at 8:00 AM on the first day (sellTime8AM)
            var firstDay = groupedByDate.First();
            var initialBuyPrice = GetPriceAtTime(firstDay, sellTime8AM, true); // Use Open price

            if (!initialBuyPrice.HasValue)
            {
                return new AnalysisResult
                {
                    NumberOfTransactions = 0,
                    DailyResults = new List<DailyResult>(),
                };
            }

            numStocks = (int)(currentAmount / initialBuyPrice.Value);
            var cashAfterInitialPurchase = currentAmount - (numStocks * initialBuyPrice.Value);
            transactionCount = 1; // Initial buy transaction

            // Process trading days (skip first day since we already bought)
            for (int i = 1; i < groupedByDate.Count; i++)
            {
                var dayGroup = groupedByDate[i];

                var sellPrice = GetPriceAtTime(dayGroup, sellTime8AM, true); // Use Open price for sell
                var buyPrice = GetPriceAtTime(dayGroup, buyTime2PM, false); // Use Close price for buy

                lastSellPrice = sellPrice;

                if (sellPrice.HasValue && buyPrice.HasValue && numStocks > 0)
                {
                    // Sell all stocks at 8:00 AM
                    cashAfterInitialPurchase += numStocks * sellPrice.Value;
                    numStocks = 0;
                    transactionCount++;
                    var midDay = cashAfterInitialPurchase;

                    // Buy stocks at 2:00 PM
                    numStocks = (int)(cashAfterInitialPurchase / buyPrice.Value);
                    cashAfterInitialPurchase -= numStocks * buyPrice.Value;
                    transactionCount++;

                    // Calculate current total value for daily result

                    decimal dailyCommission = Math.Max(0.0005m * midDay, 25m) * 2; // Buy and sell commission
                    TotalCommission += dailyCommission;

                    dailyResults.Add(new DailyResult
                    {
                        Date = dayGroup.Key,
                        ProfitOrLoss = midDay - _startAmount,
                        currentAmount = midDay,
                        numStocks = numStocks,
                        totalCommission = dailyCommission
                    });
                }
            }

            // Final sale at 3:00 PM on the last day
            if (lastSellPrice.HasValue && numStocks > 0)
            {
                cashAfterInitialPurchase += numStocks * lastSellPrice.Value;
                numStocks = 0;
                transactionCount++;
            }

            currentAmount = cashAfterInitialPurchase;

            if (transactionCount <= 1) // Only initial purchase, no trading occurred
            {
                return new AnalysisResult
                {
                    NumberOfTransactions = transactionCount,
                    DailyResults = dailyResults,
                };
            }

            var totalProfit = currentAmount - _startAmount;

            // Calculate commission : Max ( 25 DKK per transaction and 0.05% of transaction value)
            var avgTransactionValue = _startAmount; // Simplified approximation
            TotalCommission += Math.Max(0.0005m * avgTransactionValue, 25m);

            // if includeCommission is true then Amount = totalProfit else Amount = totalProfit - TotalCommission

            if (includeCommission)
                amount += currentAmount - TotalCommission; 
            else
                amount += currentAmount;

            return new AnalysisResult
            {
                Amount = amount,
                NumberOfTransactions = transactionCount,
                TotalCommission = TotalCommission,
                DailyResults = dailyResults
            };
        }

        /// <summary>
        /// Helper method to get stock price at a specific time of day
        /// </summary>
        /// <param name="dayGroup">Stock values for a specific day</param>
        /// <param name="targetTime">Target time to find price for</param>
        /// <param name="useOpenPrice">True to use Open price, false to use Close price</param>
        /// <returns>The stock price at the specified time, or null if not found</returns>
        private static decimal? GetPriceAtTime(IGrouping<DateTime, StockValue> dayGroup, TimeSpan targetTime, bool useOpenPrice)
        {
            var stockValue = dayGroup
                .Where(sv => sv.Timestamp.TimeOfDay >= targetTime)
                .OrderBy(sv => sv.Timestamp.TimeOfDay)
                .FirstOrDefault();

            return useOpenPrice ? stockValue?.Open : stockValue?.Close;
        }
    }
}