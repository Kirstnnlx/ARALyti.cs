using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ARALyti.cs.Data;
using ARALyti.cs.Models;
using ARALyti.cs.Services;
using Microsoft.Win32;

namespace ARALyti.cs.views
{
    public partial class ScanProjectView : UserControl
    {
        // Stores the full topic objects (with Id, difficulty, etc.) from the last scan
        public static List<Topic> LastDetectedTopicObjects = new List<Topic>();

        // Stores the simplified detection results (Name, Score, Level) from Roslyn analyzer
        public static List<TopicResult> LastDetectedTopics = new List<TopicResult>();

        // Keeps track of all projects scanned in this session
        public static List<Project> ScannedProjects = new List<Project>();
        public static string LastScannedFileName = "";

        // Master list of all 12 topics we track, with default values
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

        // Resets UI elements when clearing or switching away
        public void ClearScanView()
        {
            selectedFileContent = "";
            LastScannedFileName = "";

            FileNameText.Text = "No file selected";
            SelectedFileText.Text = "No file selected";
            CodePreviewText.Text = "Code preview will appear here...";

            DetectedTopicsPanel.Children.Clear();
        }

        // Updates streak display on the UI (called from MainWindow)
        public void UpdateStreakDisplay(int streak)
        {
            StreakDaysText.Text = streak.ToString();
        }

        // Opens file picker, reads the selected .cs file, and shows a preview
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

        // Shows the profile popup (linked to MainWindow)
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Window.GetWindow(this);
            if (mainWindow != null)
            {
                mainWindow.ShowProfilePopup((UIElement)sender);
            }
        }

        /// <summary>
        /// Core scanning logic:
        /// 1. Runs Roslyn-based keyword detector (returns List<TopicResult>)
        /// 2. Saves project and topic results to local database
        /// 3. Calculates overall progress score for the dashboard
        /// 4. Displays all detected topics (score > 0) without filtering
        /// </summary>
        private void StartScanningButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedFileContent))
            {
                MessageBox.Show("Please choose a C# file first.");
                return;
            }

            KeywordDetector detector = new KeywordDetector();
            // topics is List<TopicResult> (Name, Score, Level)
            var topics = detector.DetectTopics(selectedFileContent);

            // Save new project to database if not already scanned
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

            // Reset topic objects with starter list (all scores = 0)
            LastDetectedTopicObjects.Clear();
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

            // Merge detection results (using .Name and .Score)
            foreach (var detectedTopic in topics)
            {
                string topicName = detectedTopic.Name;
                int score = detectedTopic.Score;

                // Map score to status using thresholds: Strong ≥75, Developing 40-74, Weak 1-39
                string status;
                if (score >= 75)
                    status = "Strong";
                else if (score >= 40)
                    status = "Developing";
                else if (score > 0)
                    status = "Weak";
                else
                    status = "Not Started";

                var matchingTopic = LastDetectedTopicObjects.FirstOrDefault(t => t.Name == topicName);
                if (matchingTopic != null)
                {
                    matchingTopic.Score = score;
                    matchingTopic.Status = status;
                }
            }

            // Save all topics to database for this project
            int projectId = DatabaseService.GetProjectIdByFilePath(SelectedFileText.Text);
            if (projectId != -1)
            {
                foreach (var topic in LastDetectedTopicObjects)
                {
                    DatabaseService.SaveTopic(projectId, topic.Name, topic.Difficulty, topic.Status, topic.Score);
                }
                // Reload from database to ensure consistency
                LastDetectedTopicObjects = DatabaseService.GetTopicsByProjectId(projectId);
            }

            // =====================================================
            // LINE GRAPH PROGRESS RECORD
            // Save the same overall mastery score used by the donut chart.
            // This makes the line graph show progress history over time.
            // =====================================================

            var overallTopics = DatabaseService.GetOverallTopics();

            var detectedOverallTopics = overallTopics
                .Where(t => t.Status != "Not Started")
                .ToList();

            if (detectedOverallTopics.Count > 0)
            {
                double overallProgress = detectedOverallTopics.Average(t => t.Score);

                DatabaseService.SaveProgressRecord((int)Math.Round(overallProgress));
            }

            // Store raw detection results for potential external use
            LastDetectedTopics = topics;

            if (topics.Count == 0)
            {
                MessageBox.Show("No topics detected.");
                return;
            }

            // --- DISPLAY ALL DETECTED TOPICS (no filtering by score) ---
            DetectedTopicsPanel.Children.Clear();

            foreach (var topic in LastDetectedTopicObjects
                .Where(t => t.Status != "Not Started")
                .OrderByDescending(t => t.Score))
            {

                Grid row = new Grid { Margin = new Thickness(0, 0, 0, 18) };
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
                case "Strong": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#45E2A0"));
                case "Developing": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCB47"));
                case "Weak": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5FA5"));
                default: return Brushes.Gray;
            }
        }


        private Brush GetStatusBackground(string status)
        {
            switch (status)
            {
                case "Strong": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#063B2E"));
                case "Developing": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A270B"));
                case "Weak": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A0B2A"));
                default: return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A2145"));
            }
        }
    }
}