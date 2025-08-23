using StockData.Common;

namespace AktieAnalyzer
{
    public partial class StockAnalyzer : Form
    {
        public StockAnalyzer()
        {
            InitializeComponent();
            SetupListViewColumns();
        }

        private void SetupListViewColumns()
        {
            listViewResults.Clear();
            textBoxStartAmount.Text = "";
            textBoxEndAmount.Text = "";
            textBoxPL.Text = "";
            // Set the view to show details
            listViewResults.View = View.Details;

            // Add columns to the ListView
            listViewResults.Columns.Add("Stock Code", 100, HorizontalAlignment.Left);
            listViewResults.Columns.Add("Friendly Name", 150, HorizontalAlignment.Left);
            listViewResults.Columns.Add("Amount", 80, HorizontalAlignment.Right);
            listViewResults.Columns.Add("Transactions", 100, HorizontalAlignment.Right); // Added column for number of transactions
            listViewResults.Columns.Add("Recommendation", 120, HorizontalAlignment.Left);
            listViewResults.Columns.Add("Reason", 200, HorizontalAlignment.Left);
        }

        private async void StockAnalyzer_Load(object sender, EventArgs e)
        {
            // Read all stock values for all stocks into memory
            var stocks = await Helper.GetStocksFromDatabase();
            decimal totalAmount = 100.00m; // Starting amount for calculations

            foreach (var stock in stocks)
            {
                if (stock.StockCode == @"^OMXC25")
                     continue;
                var stockValues = await Helper.GetAllStockValuesFromDatabase(stock.StockCode);

                if (stockValues.Count > 0)
                {
                    // Specify the time range for analysis (e.g., last 30 days)
                    var timeRange = TimeSpan.FromDays(30);

                    var analyzer = new Analyzer(stock.StockCode, stock.FriendlyName, stockValues, timeRange);
                    var analysisResult = analyzer.AnalyzeBuyAt8SellAt14();

                    // Display analysis result in the UI
                    var listViewItem = new ListViewItem(new[]
                    {
                        stock.StockCode,
                        stock.FriendlyName,
                        analysisResult.Amount.ToString("F2"),
                        analysisResult.NumberOfTransactions.ToString(), // Display number of transactions
                        analysisResult.Recommendation,
                        analysisResult.Reason
                    });
                    totalAmount += analysisResult.Amount;
                    listViewResults.Items.Add(listViewItem);
                }
                // Sum up Amount
                textBoxStartAmount.Text = "100.00";
                textBoxEndAmount.Text = totalAmount.ToString("F2");
                textBoxPL.Text = (totalAmount - 100.00m).ToString("F2");
            }
        }
    }
}
