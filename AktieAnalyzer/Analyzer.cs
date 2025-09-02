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
        private readonly decimal _commissionPercentage;
        private readonly decimal _commissionPerTransaction;

        // Trading times
        private static readonly TimeSpan SellTime = new(8, 0, 0);
        private static readonly TimeSpan BuyTime = new(14, 0, 0);

        public Analyzer(string stockCode, string friendlyName, List<StockValue> stockValues, TimeSpan timeRange, decimal startAmount, decimal commissionPercentage = 0.0005m, decimal commissionPerTransaction = 25m)
        {
            _stockCode = stockCode;
            _friendlyName = friendlyName;
            _stockValues = stockValues;
            _timeRange = timeRange;
            _startAmount = startAmount;
            _commissionPercentage = commissionPercentage;
            _commissionPerTransaction = commissionPerTransaction;
        }

        public AnalysisResult AnalyzeBuyAt8SellAt14(bool includeCommission)
        {
            var dailyGroups = GetTradingDays();
            var portfolio = InitializePortfolio(dailyGroups.First());

            if (portfolio == null || !dailyGroups.Any())
            {
                return CreateEmptyResult();
            }

            var dailyResults = ProcessTradingDays(dailyGroups.Skip(1), portfolio);
            FinalizePortfolio(portfolio);

            return CreateFinalResult(portfolio, dailyResults, includeCommission);
        }


        // ***************** Helper routines *********************


        private List<DailyResult> ProcessTradingDays(IEnumerable<IGrouping<DateTime, StockValue>> tradingDays, TradingPortfolio portfolio)
        {
            var dailyResults = new List<DailyResult>();

            foreach (var day in tradingDays)
            {
                var sellPrice = GetPriceAtTime(day, SellTime, useOpenPrice: true);
                var buyPrice = GetPriceAtTime(day, BuyTime, useOpenPrice: false);

                portfolio.LastSellPrice = sellPrice;

                if (sellPrice.HasValue && buyPrice.HasValue && portfolio.Shares > 0)
                {
                    var dailyResult = ExecuteDayTrade(day, sellPrice.Value, buyPrice.Value, portfolio);
                    dailyResults.Add(dailyResult);
                }
            }

            return dailyResults;
        }

        private DailyResult ExecuteDayTrade(IGrouping<DateTime, StockValue> day, decimal sellPrice, decimal buyPrice, TradingPortfolio portfolio)
        {
            // Sell all shares 
            var sellAmount = portfolio.Shares * sellPrice;
            portfolio.Cash += sellAmount;
            portfolio.Shares = 0;
            portfolio.TransactionCount++;

            // Buy shares for all Cash
            portfolio.Shares = (int)(portfolio.Cash / buyPrice);
            var buyAmount = portfolio.Shares * buyPrice;
            portfolio.Cash -= buyAmount;
            portfolio.TransactionCount++;

            // Calculate commission for both transactions
            var dailyCommission = CalculateCommission(sellAmount) + CalculateCommission(buyAmount);
            portfolio.TotalCommission += dailyCommission;

            return new DailyResult
            {
                Date = day.Key,
                ProfitOrLoss = sellAmount - buyAmount,
                currentAmount = portfolio.Cash,
                numStocks = portfolio.Shares,
                totalCommission = dailyCommission
            };
        }

        private void FinalizePortfolio(TradingPortfolio portfolio)
        {
            // Final liquidation using last known sell price if still holding shares
            if (portfolio.LastSellPrice.HasValue && portfolio.Shares > 0)
            {
                var finalSellAmount = portfolio.Shares * portfolio.LastSellPrice.Value;
                portfolio.Cash += finalSellAmount;
                portfolio.Shares = 0;
                portfolio.TransactionCount++;
                portfolio.TotalCommission += CalculateCommission(finalSellAmount);
            }
        }

        private List<IGrouping<DateTime, StockValue>> GetTradingDays()
        {
            var cutoffDate = DateTime.Now.Subtract(_timeRange);
            return _stockValues
                .Where(sv => sv.Timestamp >= cutoffDate)
                .GroupBy(sv => sv.Timestamp.Date)
                .OrderBy(g => g.Key)
                .ToList();
        }

        private AnalysisResult CreateEmptyResult()
        {
            return new AnalysisResult
            {
                NumberOfTransactions = 0,
                DailyResults = new List<DailyResult>(),
            };
        }

        private TradingPortfolio? InitializePortfolio(IGrouping<DateTime, StockValue> firstDay)
        {
            var initialBuyPrice = GetPriceAtTime(firstDay, SellTime, useOpenPrice: true);
            if (!initialBuyPrice.HasValue)
            {
                return null;
            }

            var shares = (int)(_startAmount / initialBuyPrice.Value);
            var cash = _startAmount - (shares * initialBuyPrice.Value);

            return new TradingPortfolio
            {
                Cash = cash,
                Shares = shares,
                TransactionCount = 1,
                TotalCommission = 0m,
                LastSellPrice = null
            };
        }

        private AnalysisResult CreateFinalResult(TradingPortfolio portfolio, List<DailyResult> dailyResults, bool includeCommission)
        {
            if (portfolio.TransactionCount <= 1)
            {
                return new AnalysisResult
                {
                    NumberOfTransactions = portfolio.TransactionCount,
                    DailyResults = dailyResults,
                };
            }

            var finalAmount = includeCommission
                ? portfolio.Cash - portfolio.TotalCommission
                : portfolio.Cash;

            return new AnalysisResult
            {
                Amount = finalAmount,
                NumberOfTransactions = portfolio.TransactionCount,
                TotalCommission = portfolio.TotalCommission,
                DailyResults = dailyResults
            };
        }

        private decimal CalculateCommission(decimal transactionValue)
        {
            var r = (_commissionPercentage / 100) * transactionValue;
            if (r < _commissionPerTransaction)
                return _commissionPerTransaction;
            else 
                return r;
        }

        
        private static decimal? GetPriceAtTime(IGrouping<DateTime, StockValue> dayGroup, TimeSpan targetTime, bool useOpenPrice)
        {
            var stockValue = dayGroup
                .Where(sv => sv.Timestamp.TimeOfDay >= targetTime)
                .OrderBy(sv => sv.Timestamp.TimeOfDay)
                .FirstOrDefault();

            return useOpenPrice ? stockValue?.Open : stockValue?.Close;
        }

        private class TradingPortfolio
        {
            public decimal Cash { get; set; }
            public int Shares { get; set; }
            public int TransactionCount { get; set; }
            public decimal TotalCommission { get; set; }
            public decimal? LastSellPrice { get; set; }
        }
    }
}