using Caliburn.Micro;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Series;
// using OxyPlot.Wpf;
using OxyPlot.Axes;

namespace PIOprocessing
{

    public class SpotModel
    {
        public string ReportDirectory { get; set; }
        public Report Report { get; set; }

        public SortedDictionary<HandCategory, HashSet<HandType>> StrengthTree;

        public BindableCollection<HandCategory> Categories { get; set; } = new BindableCollection<HandCategory>();
        // public HashSet<string> Categories { get; set; }
        public HashSet<string> Types { get; set; }


        public PlotModel PlotModel { get; private set; }

        private HandGroup handGroup;

        public SpotModel(Report report)
        {
            Report = report;
            StrengthTree = report.HandCategories;
            foreach(HandCategory key in StrengthTree.Keys)
            {
                Categories.Add(key);
            }

            // PlotModel = new PlotModel { Title = "Example 1" };
           // PlotModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "cos(x)"));

            
        }

        public void LoadPlotModel(HandType type)
        {
            string title = $"{Report.Spot.Action} {Report.Spot.AggPos}vs{Report.Spot.CllPos} {Report.Spot.BoardType} {Report.Spot.BoardSubtype} {type}";
            PlotModel = new PlotModel { Title =  title};
            OxyPlot.Axes.LinearAxis yAxis = new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Left, MajorStep = 10, MinorStep = 10, Minimum = 0, Maximum = 100 };
            OxyPlot.Axes.LinearAxis xAxis = new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Bottom, MajorStep = 1, MinorStep = 1 };

            PlotModel.Axes.Add(xAxis);
            PlotModel.Axes.Add(yAxis);

            handGroup = Report.GetHandGroup(type);
            string[] frequencyLabels = handGroup.GetFrequencyLabels();
            foreach (string frequencyLabel in frequencyLabels)
            {
                List<DataPoint> pointsList = new List<DataPoint>();
                FrequencyValues freqValues = handGroup.GetFrequencyValues(frequencyLabel);
                foreach (int order in freqValues.GetHandStrengthOrders()) {
                    pointsList.Add(new DataPoint(order, freqValues.GetFrequency(order)));
                }
                LineSeries series = new LineSeries()
                {
                    InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline,
                    Title = frequencyLabel,
                    ItemsSource = pointsList,
                    
                    MarkerSize = 5,
                    MarkerType = MarkerType.Circle
                };

                PlotModel.Series.Add(series);
            }
        }


    }
    
}
