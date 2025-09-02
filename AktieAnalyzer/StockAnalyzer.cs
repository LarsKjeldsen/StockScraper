using StockData.Common;
using System.Numerics;

namespace AktieAnalyzer
{
    public partial class StockAnalyzer : Form
    {
        private ListView listViewDailyResults;
        private Dictionary<string, AnalysisResult> analysisResults; // Store analysis results by stock code

        public StockAnalyzer()
        {
            InitializeComponent();
            SetupListViewColumns();
            InitializeDailyResultsListView(); // Ensure the daily results ListView is initialized
            analysisResults = new Dictionary<string, AnalysisResult>(); // Initialize the dictionary
            textBoxStartAmount.Text = "10.000";
        }

        private void SetupListViewColumns()
        {
            // Set the view to show details
            listViewResults.View = View.Details;

            // Add columns to the ListView
            listViewResults.Columns.Add("Stock Code", 100, HorizontalAlignment.Left);
            listViewResults.Columns.Add("Friendly Name", 150, HorizontalAlignment.Left);
            listViewResults.Columns.Add("EndAmount", 80, HorizontalAlignment.Right);
            listViewResults.Columns.Add("Transactions", 100, HorizontalAlignment.Right); // Added column for number of transactions
            listViewResults.Columns.Add("EndStocks", 120, HorizontalAlignment.Left);
        }

        private void InitializeDailyResultsListView()
        {
            if (listViewDailyResults == null)
            {
                listViewDailyResults = new ListView
                {
                    View = View.Details,
                    FullRowSelect = true,
                    GridLines = true,
                    Dock = DockStyle.Bottom,
                    Height = 200
                };

                listViewDailyResults.Columns.Add("Date", 100, HorizontalAlignment.Left);
                listViewDailyResults.Columns.Add("Start Amount", 120, HorizontalAlignment.Right);
                listViewDailyResults.Columns.Add("End Amount", 120, HorizontalAlignment.Right);
                listViewDailyResults.Columns.Add("Profit/Loss", 120, HorizontalAlignment.Right);

                Controls.Add(listViewDailyResults);
            }
        }

        private async void StockAnalyzer_Load(object sender, EventArgs e)
        {
            listViewResults.Items.Clear();
            textBoxEndAmount.Text = "";
            textBoxPL.Text = "";
            analysisResults.Clear(); // Clear previous analysis results

            // Parse the start amount from the textbox
            if (!decimal.TryParse(textBoxStartAmount.Text, out decimal startAmount))
            {
                MessageBox.Show("Invalid start amount. Please enter a valid number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            textBoxStartAmount.Text = startAmount.ToString("N0");

            decimal totalAmount = startAmount;
            decimal totalCommission = 0.00m;

            // Read all stock values for all stocks into memory
            var stocks = await Helper.GetStocksFromDatabase();

            foreach (var stock in stocks)
            {
                if (stock.StockCode == @"^OMXC25")
                    continue;

                var stockValues = await Helper.GetAllStockValuesFromDatabase(stock.StockCode);

                if (stockValues.Count > 0)
                {
                    // Specify the time range for analysis (e.g., last 1 year)
                    var timeRange = TimeSpan.FromDays(365);

                    var analyzer = new Analyzer(stock.StockCode, stock.FriendlyName, stockValues, timeRange, startAmount);
                    var analysisResult = analyzer.AnalyzeBuyAt8SellAt14();

                    // Store the analysis result for later use
                    analysisResults[stock.StockCode] = analysisResult;

                    // Display analysis result in the UI
                    var listViewItem = new ListViewItem(new[]
                    {
                        stock.StockCode,
                        stock.FriendlyName,
                        analysisResult.Amount.ToString("N0"), // Display amount as decimal with thousand separators
                        analysisResult.NumberOfTransactions.ToString("N0"), // Display number of transactions as integer with thousand separators
                    });
                    totalAmount += analysisResult.Amount;
                    totalCommission += analysisResult.TotalCommission;
                    listViewResults.Items.Add(listViewItem);
                }

                // Update summary fields
                textBoxEndAmount.Text = totalAmount.ToString("N0"); // Display total amount as decimal with thousand separators
                textBoxPL.Text = (totalAmount - startAmount).ToString("N0"); // Display profit/loss as decimal with thousand separators
                textBoxCommission.Text = totalCommission.ToString("N0"); // Display commission as decimal with thousand separators
            }
        }

        private void DisplayDailyResults(List<DailyResult> dailyResults)
        {
            foreach (var dailyResult in dailyResults)
            {
                var listViewItem = new ListViewItem(new[]
                {
                    dailyResult.Date.ToShortDateString(),
                    dailyResult.currentAmount.ToString("N0"),
                    dailyResult.numStocks.ToString("N0"),
                    dailyResult.ProfitOrLoss.ToString("N0")
                });
                listViewDailyResults.Items.Add(listViewItem);
            }
        }

        private void listViewResults_ItemActivate(object sender, EventArgs e)
        {
            if (listViewResults.SelectedItems.Count > 0)
            {
                var selectedItem = listViewResults.SelectedItems[0];
                var stockCode = selectedItem.SubItems[0].Text;

                // Use the already calculated analysis result instead of recalculating
                if (analysisResults.TryGetValue(stockCode, out var analysisResult))
                {
                    // Open the graph form with the existing analysis result
                    var graphForm = new DailyResultGraphForm(analysisResult.DailyResults);
                    graphForm.ShowDialog();
                }
                else
                {
                   MessageBox.Show("Analysis result not found for the selected stock.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
