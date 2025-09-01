using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using OxyPlot.Axes;
using OxyPlot.Legends;
using System.Linq;

namespace AktieAnalyzer
{
    public partial class DailyResultGraphForm : Form
    {
        public DailyResultGraphForm(List<DailyResult> dailyResults)
        {
            InitializeComponent();
            InitializeChart(dailyResults);
        }

        private void InitializeChart(List<DailyResult> dailyResults)
        {
            var plotView = new PlotView
            {
                Dock = DockStyle.Fill
            };

            var model = new PlotModel 
            { 
                Title = "Daily Profit/Loss Over Time",
                Background = OxyColors.White
            };

            // Configure axes with more date labels
            model.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Date",
                StringFormat = "MM-dd",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.LightGray,
                MinorGridlineColor = OxyColors.LightGray,
                IntervalType = DateTimeIntervalType.Days,
                MajorStep = 7, // Major tick every 7 days (weekly)
                MinorStep = 1, // Minor tick every day
                Angle = -45 // Rotate labels for better readability
            });

            // Primary Y-axis for daily profit/loss (left side)
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Daily Profit/Loss",
                Key = "DailyAxis",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.LightGray,
                MinorGridlineColor = OxyColors.LightGray,
                TitleColor = OxyColors.Blue,
                TextColor = OxyColors.Blue
            });

            // Secondary Y-axis for cumulative total value (right side)
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Right,
                Title = "Cumulative Total Value",
                Key = "CumulativeAxis",
                MajorGridlineStyle = LineStyle.None, // Don't show gridlines for secondary axis to avoid confusion
                MinorGridlineStyle = LineStyle.None,
                TitleColor = OxyColors.Red,
                TextColor = OxyColors.Red
            });

            // Create scatter series for daily profit/loss with green and red coloring
            var dailyProfitSeries = new ScatterSeries
            {
                Title = "Profit (> 0)",
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerFill = OxyColors.Green,
                YAxisKey = "DailyAxis"
            };
            var dailyLossSeries = new ScatterSeries
            {
                Title = "Loss (< 0)",
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerFill = OxyColors.Red,
                YAxisKey = "DailyAxis"
            };

            // Create line series for cumulative sum
            var cumulativeLineSeries = new LineSeries
            {
                Title = "Cumulative Total Value",
                Color = OxyColors.Blue, // Changed from Red to Blue
                StrokeThickness = 3,
                MarkerType = MarkerType.Square,
                MarkerSize = 3,
                MarkerFill = OxyColors.Blue, // Changed from Red to Blue
                YAxisKey = "CumulativeAxis" // Bind to secondary Y-axis
            };

            // Sort daily results by date to ensure proper cumulative calculation
            var sortedResults = dailyResults.OrderBy(dr => dr.Date).ToList();
            
            decimal cumulativeValue = sortedResults.FirstOrDefault()?.StartAmount ?? 10000; // Use the actual start amount

            // Add data points
            foreach (var dailyResult in sortedResults)
            {
                var dateValue = DateTimeAxis.ToDouble(dailyResult.Date);
                var profitOrLoss = (double)dailyResult.ProfitOrLoss;

                if (profitOrLoss > 0)
                {
                    dailyProfitSeries.Points.Add(new ScatterPoint(dateValue, profitOrLoss));
                }
                else if (profitOrLoss < 0)
                {
                    dailyLossSeries.Points.Add(new ScatterPoint(dateValue, profitOrLoss));
                }
                // If exactly zero, you can choose to add to either or skip

                // Use the actual end amount from daily result for cumulative value
                cumulativeLineSeries.Points.Add(new DataPoint(
                    dateValue,
                    (double)dailyResult.EndAmount));
            }

            model.Series.Add(dailyProfitSeries);
            model.Series.Add(dailyLossSeries);
            model.Series.Add(cumulativeLineSeries);
            
            // Configure legend for OxyPlot 2.x
            model.Legends.Add(new Legend
            {
                LegendPosition = LegendPosition.TopRight,
                LegendPlacement = LegendPlacement.Outside,
                LegendOrientation = LegendOrientation.Vertical
            });
            
            plotView.Model = model;

            Controls.Add(plotView);
        }
    }
}