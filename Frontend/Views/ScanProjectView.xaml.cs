using ARALyti.cs.Services;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using ARALyti.cs.Models;
using ARALyti.cs.Data;
using System.Collections.Generic;

namespace ARALyti.cs.views
{
    public partial class ScanProjectView : UserControl
    {
        public static List<Topic> LastDetectedTopicObjects = new List<Topic>();
        public static Dictionary<string, int> LastDetectedTopics = new Dictionary<string, int>();
        public static List<Project> ScannedProjects = new List<Project>();
        public static string LastScannedFileName = "";

        public static List<Topic> StarterTopics = new List<Topic>
        {
            new Topic { TopicId = "T001", Name = "Object-Oriented Programming", Difficulty = "Medium", Status = "Not Started", Score = 0 },
            new Topic { TopicId = "T002", Name = "Classes and Objects", Difficulty = "Easy", Status = "Not Started", Score = 0 },
            new Topic { TopicId = "T003", Name = "Methods and Functions", Difficulty = "Easy", Status = "Not Started", Score = 0 },
            new Topic { TopicId = "T004", Name = "Conditional Statements", Difficulty = "Easy", Status = "Not Started", Score = 0 },
            new Topic { TopicId = "T005", Name = "Loops", Difficulty = "Easy", Status = "Not Started", Score = 0 },
            new Topic { TopicId = "T006", Name = "Arrays", Difficulty = "Medium", Status = "Not Started", Score = 0 },
            new Topic { TopicId = "T007", Name = "Collections", Difficulty = "Medium", Status = "Not Started", Score = 0 },
            new Topic { TopicId = "T008", Name = "Exception Handling", Difficulty = "Medium", Status = "Not Started", Score = 0 },
            new Topic { TopicId = "T009", Name = "Inheritance", Difficulty = "Hard", Status = "Not Started", Score = 0 },
            new Topic { TopicId = "T010", Name = "Encapsulation", Difficulty = "Medium", Status = "Not Started", Score = 0 },
            new Topic { TopicId = "T011", Name = "Recursion", Difficulty = "Hard", Status = "Not Started", Score = 0 },
            new Topic { TopicId = "T012", Name = "File Handling", Difficulty = "Medium", Status = "Not Started", Score = 0 }
        };

        private string selectedFileContent = "";
        public ScanProjectView()
        {
            InitializeComponent();
        }

        public void ClearScanView()
        {
            selectedFileContent = "";
            LastScannedFileName = "";

            FileNameText.Text = "No file selected";
            SelectedFileText.Text = "No file selected";
            CodePreviewText.Text = "Code preview will appear here...";

            DetectedTopicsPanel.Children.Clear();
        }

        public void UpdateStreakDisplay(int streak)
        {
            StreakDaysText.Text = streak.ToString();
        }

        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "C# Files (*.cs)|*.cs|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                DetectedTopicsPanel.Children.Clear();
                CodePreviewText.Text = "Code preview will appear here...";

                SelectedFileText.Text = openFileDialog.FileName;
                FileNameText.Text = Path.GetFileName(openFileDialog.FileName);
                LastScannedFileName = Path.GetFileName(openFileDialog.FileName);
                selectedFileContent = File.ReadAllText(openFileDialog.FileName);
                var lines = selectedFileContent.Split('\n');

                int maxLines = 20;
                string preview = "";

                for (int i = 0; i < lines.Length && i < maxLines; i++)
                {
                    preview += lines[i] + "\n";
                }

                CodePreviewText.Text = preview;
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Window.GetWindow(this);

            if (mainWindow != null)
            {
                mainWindow.ShowProfilePopup((UIElement)sender);
            }
        }

        private void StartScanningButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedFileContent))
            {
                MessageBox.Show("Please choose a C# file first.");
                return;
            }

            KeywordDetector detector = new KeywordDetector();
            var topics = detector.DetectTopics(selectedFileContent);

            bool projectExists = ScannedProjects.Any(p => p.FilePath == SelectedFileText.Text);

            if (!projectExists)
            {
                Project newProject = new Project
                {
                    ProjectId = $"P{ScannedProjects.Count + 1:000}",
                    Title = LastScannedFileName,
                    FilePath = SelectedFileText.Text,
                    Status = "Scanned"
                };

                ScannedProjects.Add(newProject);

                DatabaseService.SaveProject(newProject.Title, newProject.FilePath);
            }

            LastDetectedTopicObjects.Clear();

            // start from all 12 starter topics
            foreach (var starterTopic in StarterTopics)
            {
                LastDetectedTopicObjects.Add(new Topic
                {
                    TopicId = starterTopic.TopicId,
                    Name = starterTopic.Name,
                    Difficulty = starterTopic.Difficulty,
                    Status = "Not Started",
                    Score = 0
                });
            }

            foreach (var detectedTopic in topics)
            {
                string topicName = detectedTopic.Key;
                int score = detectedTopic.Value;

                string status = "Weak";

                if (score >= 60)
                    status = "Strong";
                else if (score >= 30)
                    status = "Developing";
                else if (score > 0)
                    status = "Weak";
                else
                    status = "Not Started";

                var matchingTopic = LastDetectedTopicObjects
                    .FirstOrDefault(t => t.Name == topicName);

                if (matchingTopic != null)
                {
                    matchingTopic.Score = score;
                    matchingTopic.Status = status;
                }
            }

            int projectId = DatabaseService.GetProjectIdByFilePath(SelectedFileText.Text);

            if (projectId != -1)
            {
                foreach (var topic in LastDetectedTopicObjects)
                {
                    DatabaseService.SaveTopic(
                        projectId,
                        topic.Name,
                        topic.Difficulty,
                        topic.Status,
                        topic.Score
                    );
                }

                LastDetectedTopicObjects = DatabaseService.GetTopicsByProjectId(projectId);
            }

            var detectedTopics = LastDetectedTopicObjects
                .Where(t => t.Status != "Not Started")
                .ToList();

            if (detectedTopics.Count > 0)
            {
                double average = detectedTopics.Average(t => t.Score);

                int projectCount = ScanProjectView.ScannedProjects.Count;
                double adjustedScore = average * (projectCount / (projectCount + 2.0));

                // usage/activity bonus based on detected topics
                int scanBonus = Math.Min(detectedTopics.Count * 2, 20);

                // final daily learning activity score
                int progressScore = (int)Math.Min(100, adjustedScore + scanBonus);

                DatabaseService.SaveProgressRecord(progressScore);
            }


            LastDetectedTopics = topics;

            if (topics.Count == 0)
            {
                MessageBox.Show("No topics detected.");
                return;
            }

            DetectedTopicsPanel.Children.Clear();

            foreach (var topic in LastDetectedTopicObjects
                .Where(t => t.Status != "Not Started")
                .OrderByDescending(t => t.Score))
            {
                Grid row = new Grid
                {
                    Margin = new Thickness(0, 0, 0, 18)
                };

                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95) });

                StackPanel leftPanel = new StackPanel();

                TextBlock nameText = new TextBlock
                {
                    Text = topic.Name,
                    Foreground = Brushes.White,
                    FontSize = 15,
                    Margin = new Thickness(0, 0, 0, 8)
                };

                ProgressBar progressBar = new ProgressBar
                {
                    Value = topic.Score,
                    Maximum = 100,
                    Height = 7,
                    Margin = new Thickness(0, 0, 12, 0),
                    Foreground = GetStatusColor(topic.Status),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#151B45"))
                };

                leftPanel.Children.Add(nameText);
                leftPanel.Children.Add(progressBar);

                TextBlock scoreText = new TextBlock
                {
                    Text = topic.Score.ToString(),
                    Foreground = Brushes.White,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Border statusBadge = new Border
                {
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(12, 7, 12, 7),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Background = GetStatusBackground(topic.Status)
                };

                TextBlock statusText = new TextBlock
                {
                    Text = topic.Status,
                    Foreground = GetStatusColor(topic.Status),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold
                };

                statusBadge.Child = statusText;

                Grid.SetColumn(leftPanel, 0);
                Grid.SetColumn(scoreText, 1);
                Grid.SetColumn(statusBadge, 2);

                row.Children.Add(leftPanel);
                row.Children.Add(scoreText);
                row.Children.Add(statusBadge);

                DetectedTopicsPanel.Children.Add(row);
            }
        }

        private Brush GetStatusColor(string status)
        {
            switch (status)
            {
                case "Strong":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#45E2A0"));
                case "Developing":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCB47"));
                case "Weak":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5FA5"));
                default:
                    return Brushes.Gray;
            }
        }

        private Brush GetStatusBackground(string status)
        {
            switch (status)
            {
                case "Strong":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#063B2E"));
                case "Developing":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A270B"));
                case "Weak":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A0B2A"));
                default:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A2145"));
            }
        }
    }
}