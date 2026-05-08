using ARALyti.cs.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ARALyti.cs.views
{
    public partial class TopicsView : UserControl
    {

        private List<Topic> allTopics = new List<Topic>();
        private int currentPage = 1;
        private const int TopicsPerPage = 6;

        public TopicsView()
        {
            InitializeComponent();
            LoadTopics();
        }

        public void UpdateStreakDisplay(int streak)
        {
            StreakDaysText.Text = streak.ToString();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Window.GetWindow(this);

            if (mainWindow != null)
            {
                mainWindow.ShowProfilePopup((UIElement)sender);
            }
        }

        public void LoadTopics()
        {
            TopicsPanel.Children.Clear();

            var topicsToDisplay = ScanProjectView.LastDetectedTopicObjects.Count > 0
                ? ScanProjectView.LastDetectedTopicObjects
                : ScanProjectView.StarterTopics;

            topicsToDisplay = ScanProjectView.StarterTopics
                .Select(starter =>
                {
                    var match = ScanProjectView.LastDetectedTopicObjects
                        .FirstOrDefault(t => t.Name == starter.Name);

                    return match ?? new Topic
                    {
                        TopicId = starter.TopicId,
                        Name = starter.Name,
                        Difficulty = starter.Difficulty,
                        Status = "Not Started",
                        Score = 0
                    };
                })
                .ToList();

            topicsToDisplay = topicsToDisplay
                .OrderBy(t => t.TopicId)
                .ToList();

            int total = topicsToDisplay.Count(t => t.Status != "Not Started");
            int strong = 0;
            int developing = 0;
            int weak = 0;

            if (topicsToDisplay.Count == 0)
            {
                TotalTopicsText.Text = "0";
                StrongCountText.Text = "0";
                DevelopingCountText.Text = "0";
                WeakCountText.Text = "0";
                TopicsFooterText.Text = "Showing 0 topics";
                return;
            }

            allTopics = topicsToDisplay;

            int totalPages = (int)Math.Ceiling(allTopics.Count / (double)TopicsPerPage);

            if (currentPage > totalPages)
                currentPage = 1;

            var pagedTopics = allTopics
                .Skip((currentPage - 1) * TopicsPerPage)
                .Take(TopicsPerPage)
                .ToList();

            int index = 1;

            foreach (var topic in pagedTopics)
            {
                var starterMatch = ScanProjectView.StarterTopics
                    .FirstOrDefault(t => t.Name == topic.Name);

                string topicId = starterMatch?.TopicId ?? "N/A";
                string topicName = topic.Name;
                int score = topic.Score;
                string status = topic.Status;
                string difficulty = topic.Difficulty;

                if (status == "Strong")
                    strong++;
                else if (status == "Weak")
                    weak++;
                else if (status == "Developing")
                    developing++;


                Border rowBorder = new Border
                {
                    Background = (index % 2 == 1)
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0A0F28"))
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10163A")),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 1, 0, 0)
                };

                Grid rowGrid = new Grid();
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(170) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

                TextBlock idText = new TextBlock
                {
                    Text = topicId,
                    Foreground = Brushes.White
                };

                TextBlock nameText = new TextBlock
                {
                    Text = topicName,
                    Foreground = Brushes.White
                };
                Grid.SetColumn(nameText, 1);

                Brush difficultyBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCB47"));
                if (difficulty == "Easy")
                    difficultyBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#57D9FF"));
                else if (difficulty == "Hard")
                    difficultyBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C86EFF"));

                Border difficultyBadge = new Border
                {
                    Background = difficulty == "Easy"
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#073B5A"))
                        : difficulty == "Hard"
                            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A0B4F"))
                            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A270B")),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(10, 4, 10, 4),
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                TextBlock difficultyText = new TextBlock
                {
                    Text = difficulty,
                    Foreground = difficultyBrush,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold
                };

                difficultyBadge.Child = difficultyText;
                Grid.SetColumn(difficultyBadge, 2);

                Brush statusBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9AA3C7"));

                if (status == "Strong")
                    statusBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#45E2A0"));
                else if (status == "Weak")
                    statusBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5FA5"));
                else if (status == "Developing")
                    statusBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCB47"));

                Border statusBadge = new Border
                {
                    Background = status == "Strong"
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#063B2E"))
                        : status == "Weak"
                            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A0B2A"))
                            : status == "Developing"
                                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A270B"))
                                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A2145")),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(10, 4, 10, 4),
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                TextBlock statusText = new TextBlock
                {
                    Text = status,
                    Foreground = statusBrush,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold
                };

                statusBadge.Child = statusText;
                Grid.SetColumn(statusBadge, 3);

                TextBlock scoreText = new TextBlock
                {
                    Text = $"{score}%",
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold
                };
                Grid.SetColumn(scoreText, 4);

                rowGrid.Children.Add(idText);
                rowGrid.Children.Add(nameText);
                rowGrid.Children.Add(difficultyBadge);
                rowGrid.Children.Add(statusBadge);
                rowGrid.Children.Add(scoreText);

                rowBorder.Child = rowGrid;
                TopicsPanel.Children.Add(rowBorder);

                index++;
            }

            TotalTopicsText.Text = total.ToString();
            StrongCountText.Text = strong.ToString();
            DevelopingCountText.Text = developing.ToString();
            WeakCountText.Text = weak.ToString();

            int showingCount = pagedTopics.Count;

            TopicsFooterText.Text =
                $"Showing {showingCount} topics • Page {currentPage} of {totalPages}";

            PageOneButton.Background = currentPage == 1
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5B3DF5"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#11183A"));

            PageTwoButton.Background = currentPage == 2
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5B3DF5"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#11183A"));

        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadTopics();
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling(allTopics.Count / (double)TopicsPerPage);

            if (currentPage < totalPages)
            {
                currentPage++;
                LoadTopics();
            }
        }

        private void PageOneButton_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 1;
            LoadTopics();
        }

        private void PageTwoButton_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 2;
            LoadTopics();
        }
    }
}