using System.Linq;
using System.Windows.Controls;

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

            int totalTopics = topics.Count;

            DashboardTotalTopicsCountText.Text = totalTopics.ToString();

            if (totalTopics == 0)
            {
                DashboardLevelText.Text = "Beginner";
                RecommendationTitleText.Text = "No scan results yet";
                RecommendationDescriptionText.Text = "Scan a C# project first to see your learning recommendation.";

                if (FindName("DashboardProgressText") is TextBlock emptyProgressText)
                {
                    emptyProgressText.Text = "0%";
                }

                return;
            }

            double averageScore = topics.Average(t => t.Score);

            if (averageScore >= 80)
                DashboardLevelText.Text = "Advanced";
            else if (averageScore >= 50)
                DashboardLevelText.Text = "Intermediate";
            else
                DashboardLevelText.Text = "Beginner";

            var weakestTopic = topics.OrderBy(t => t.Score).First();

            RecommendationTitleText.Text = $"Focus on {weakestTopic.Name}";
            RecommendationDescriptionText.Text =
                $"Your lowest detected topic is {weakestTopic.Name} with a score of {weakestTopic.Score}%. Practice this area more to improve your overall progress.";

            if (FindName("DashboardProgressText") is TextBlock progressText)
            {
                progressText.Text = $"{(int)averageScore}%";
            }
        }
    }
}