using ARALyti.cs.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;

namespace ARALyti.cs.views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            LoadDashboardData();
        }

        public void LoadDashboardData()
        {
            var topics = ScanProjectView.LastDetectedTopicObjects;

            var detectedTopics = topics
                .Where(t => t.Status != "Not Started")
                .ToList();

            DashboardTotalTopicsCountText.Text = detectedTopics.Count.ToString();
            DashboardProjectsScannedText.Text = ScanProjectView.ScannedProjects.Count.ToString();
            DashboardDiaryEntriesText.Text = ProjectDiaryView.DiaryEntries.Count.ToString();

            if (detectedTopics.Count == 0)
            {
                DashboardLevelText.Text = "Beginner";
                RecommendationTitleText.Text = "No scan results yet";
                RecommendationDescriptionText.Text = "Scan a C# project first to see your learning recommendation.";
                DashboardProgressText.Text = "0%";

                UpdateProgressChart(0);
                LoadDashboardTopics(topics);
                UpdateProgressLineChart();
                return;
            }

            double averageScore = detectedTopics.Average(t => t.Score);

            int projectCount = ScanProjectView.ScannedProjects.Count;
            double adjustedScore = averageScore * (projectCount / (projectCount + 2.0));

            if (adjustedScore >= 70)
                DashboardLevelText.Text = "Advanced";
            else if (adjustedScore >= 40)
                DashboardLevelText.Text = "Intermediate";
            else
                DashboardLevelText.Text = "Beginner";

            averageScore = adjustedScore;

            var weakestTopic = detectedTopics
                .OrderBy(t => t.Score)
                .First();

            RecommendationTitleText.Text = $"Focus on {weakestTopic.Name}";
            RecommendationDescriptionText.Text =
                $"Your lowest detected topic is {weakestTopic.Name} with a score of {weakestTopic.Score}%. Practice this area more to improve your overall progress.";

            DashboardProgressText.Text = $"{(int)averageScore}%";

            UpdateProgressChart(averageScore);
            LoadDashboardTopics(topics);
            UpdateProgressLineChart();
        }

        private void UpdateProgressChart(double progress)
        {
            double remaining = 100 - progress;

            ProgressChart.Series = new ISeries[]
            {
                new PieSeries<double>
                {
                    Values = new double[] { progress },
                    Fill = new SolidColorPaint(SKColor.Parse("#6A3EFF")),
                    Stroke = null,
                    DataLabelsSize = 0,
                    MaxRadialColumnWidth = 18
                },
                new PieSeries<double>
                {
                    Values = new double[] { remaining },
                    Fill = new SolidColorPaint(SKColor.Parse("#1E2A59")),
                    Stroke = null,
                    DataLabelsSize = 0,
                    MaxRadialColumnWidth = 18
                }
            };
        }

        private void UpdateProgressLineChart()
        {
            var data = DatabaseService.GetRecentProgressWithDates();

            var values = data.Select(d => d.Score).ToList();
            var labels = data.Select(d => d.Date).ToList();

            if (values.Count == 0)
            {
                values = new List<double> { 0 };
                labels = new List<string> { "No Data" };
            }

            ProgressLineChart.Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = values,
                    Stroke = new SolidColorPaint(SKColor.Parse("#6A3EFF"), 4),
                    GeometrySize = 10,
                    GeometryFill = new SolidColorPaint(SKColor.Parse("#6A3EFF")),
                    Fill = null,
                    LineSmoothness = 0.7
                }
                    };

                    ProgressLineChart.XAxes = new Axis[]
                    {
                new Axis
                {
                    Labels = labels,
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#AAAAAA")),
                    SeparatorsPaint = null
                }
                    };

            ProgressLineChart.YAxes = new Axis[]
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#AAAAAA")),
                    SeparatorsPaint = null,
                    MinLimit = -10,
                    MaxLimit = 100
                }
            };
        }

        private void LoadDashboardTopics(System.Collections.Generic.List<ARALyti.cs.Models.Topic> topics)
        {
            DashboardTopicsPanel.Children.Clear();

            foreach (var topic in topics
                .Where(t => t.Status != "Not Started")
                .OrderByDescending(t => t.Score)
                .Take(4))
            {
                StackPanel container = new StackPanel
                {
                    Margin = new System.Windows.Thickness(0, 12, 0, 0)
                };

                TextBlock nameText = new TextBlock
                {
                    Text = topic.Name,
                    Foreground = Brushes.White,
                    FontSize = 17
                };

                ProgressBar progressBar = new ProgressBar
                {
                    Value = topic.Score,
                    Maximum = 100,
                    Height = 10,
                    Margin = new System.Windows.Thickness(0, 6, 0, 0)
                };

                TextBlock detailsText = new TextBlock
                {
                    Text = $"{topic.Score}%  •  {topic.Status}",
                    Foreground = GetStatusColor(topic.Status),
                    Margin = new System.Windows.Thickness(0, 4, 0, 0)
                };

                container.Children.Add(nameText);
                container.Children.Add(progressBar);
                container.Children.Add(detailsText);

                DashboardTopicsPanel.Children.Add(container);
            }
        }

        private Brush GetStatusColor(string status)
        {
            switch (status)
            {
                case "Strong":
                    return Brushes.LightGreen;
                case "Developing":
                    return Brushes.Gold;
                case "Weak":
                    return Brushes.IndianRed;
                case "Not Started":
                    return Brushes.Gray;
                default:
                    return Brushes.White;
            }
        }
    }
}