using ARALyti.cs.Services;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using ARALyti.cs.Models;
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

        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "C# Files (*.cs)|*.cs|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
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

                string status = "Developing";
                if (score >= 80)
                    status = "Strong";
                else if (score < 40)
                    status = "Weak";

                var matchingTopic = LastDetectedTopicObjects
                    .FirstOrDefault(t => t.Name == topicName);

                if (matchingTopic != null)
                {
                    matchingTopic.Score = score;
                    matchingTopic.Status = status;
                }
            }

            LastDetectedTopics = topics;

            if (topics.Count == 0)
            {
                MessageBox.Show("No topics detected.");
                return;
            }

            DetectedTopicsPanel.Children.Clear();

            foreach (var topic in topics)
            {
                string topicName = topic.Key;
                int score = topic.Value;

                string status = "Developing";
                if (score >= 80)
                    status = "Strong";
                else if (score < 40)
                    status = "Weak";

                StackPanel topicItem = new StackPanel
                {
                    Margin = new Thickness(0, 10, 0, 0)
                };

                Grid topicRow = new Grid();
                topicRow.ColumnDefinitions.Add(new ColumnDefinition());
                topicRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                topicRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });

                TextBlock topicNameText = new TextBlock
                {
                    Text = topicName,
                    Foreground = Brushes.White,
                    FontSize = 16
                };

                TextBlock scoreText = new TextBlock
                {
                    Text = $"{score}%",
                    Foreground = Brushes.White,
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Border statusBadge = new Border
                {
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(10, 4, 10, 4),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                TextBlock statusText = new TextBlock
                {
                    Text = status,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                if (status == "Strong")
                {
                    statusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0B3A33"));
                    statusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#45E2A0"));
                }
                else if (status == "Weak")
                {
                    statusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A0B2A"));
                    statusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5FA5"));
                }
                else
                {
                    statusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A270B"));
                    statusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCB47"));
                }

                statusBadge.Child = statusText;

                Grid.SetColumn(topicNameText, 0);
                Grid.SetColumn(scoreText, 1);
                Grid.SetColumn(statusBadge, 2);

                topicRow.Children.Add(topicNameText);
                topicRow.Children.Add(scoreText);
                topicRow.Children.Add(statusBadge);

                ProgressBar progress = new ProgressBar
                {
                    Value = score,
                    Maximum = 100,
                    Height = 10,
                    Margin = new Thickness(0, 8, 0, 0)
                };

                topicItem.Children.Add(topicRow);
                topicItem.Children.Add(progress);

                DetectedTopicsPanel.Children.Add(topicItem);
            }
        }
    }
}