using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

                LoadDashboardTopics(topics);
                return;
            }

            double averageScore = detectedTopics.Average(t => t.Score);

            if (averageScore >= 80)
                DashboardLevelText.Text = "Advanced";
            else if (averageScore >= 50)
                DashboardLevelText.Text = "Intermediate";
            else
                DashboardLevelText.Text = "Beginner";

            var weakestTopic = detectedTopics
                .OrderBy(t => t.Score)
                .First();

            RecommendationTitleText.Text = $"Focus on {weakestTopic.Name}";
            RecommendationDescriptionText.Text =
                $"Your lowest detected topic is {weakestTopic.Name} with a score of {weakestTopic.Score}%. Practice this area more to improve your overall progress.";

            DashboardProgressText.Text = $"{(int)averageScore}%";

            LoadDashboardTopics(topics);
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
                    Margin = new Thickness(0, 12, 0, 0)
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
                    Margin = new Thickness(0, 6, 0, 0)
                };

                TextBlock detailsText = new TextBlock
                {
                    Text = $"{topic.Score}%  •  {topic.Status}",
                    Foreground = GetStatusColor(topic.Status),
                    Margin = new Thickness(0, 4, 0, 0)
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