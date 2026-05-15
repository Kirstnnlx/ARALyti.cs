using ARALyti.cs.Data;
using ARALyti.cs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ARALyti.cs.views
{
    public partial class ProjectDiaryView : UserControl
    {
        public static List<ProjectDiaryEntry> DiaryEntries = new List<ProjectDiaryEntry>();

        public ProjectDiaryView()
        {
            InitializeComponent();
            LoadProjectSelector();
            LoadEntries();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Window.GetWindow(this);
            mainWindow?.ShowProfilePopup((UIElement)sender);
        }

        public void UpdateStreakDisplay(int streak)
        {
            StreakDaysText.Text = streak.ToString();
        }

        public void LoadProjectSelector()
        {
            ProjectSelectorComboBox.Items.Clear();
            DeleteProjectSelectorComboBox.Items.Clear();

            foreach (var project in ScanProjectView.ScannedProjects)
            {
                ProjectSelectorComboBox.Items.Add(project.Title);
                DeleteProjectSelectorComboBox.Items.Add(project.Title);
            }
        }

        private void AddEntryCardButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanProjectView.ScannedProjects.Count == 0)
            {
                MessageBox.Show("Please scan a project first before adding a diary entry.");
                return;
            }

            ProjectSelectorComboBox.SelectedIndex = -1;
            DiaryInputTextBox.Text = DiaryInputTextBox.Tag.ToString();
            DiaryInputTextBox.Foreground = Brushes.Gray;

            AddEntryOverlay.Visibility = Visibility.Visible;
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            AddEntryOverlay.Visibility = Visibility.Collapsed;
        }

        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AddEntryOverlay.Visibility = Visibility.Collapsed;
        }

        private void DeleteProjectCardButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanProjectView.ScannedProjects.Count == 0)
            {
                MessageBox.Show("No projects to delete.");
                return;
            }

            DeleteProjectSelectorComboBox.SelectedIndex = -1;
            DeleteProjectOverlay.Visibility = Visibility.Visible;
        }

        private void CloseDeleteModal_Click(object sender, RoutedEventArgs e)
        {
            DeleteProjectOverlay.Visibility = Visibility.Collapsed;
        }

        private void DeleteOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DeleteProjectOverlay.Visibility = Visibility.Collapsed;
        }

        private void ConfirmDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DeleteProjectSelectorComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a project to delete.");
                return;
            }

            string selectedTitle = DeleteProjectSelectorComboBox.SelectedItem.ToString() ?? "";
            var project = ScanProjectView.ScannedProjects
                .FirstOrDefault(p => p.Title == selectedTitle);

            if (project == null) return;

            DeleteProjectOverlay.Visibility = Visibility.Collapsed;

            DatabaseService.DeleteProjectByFilePath(project.FilePath);
            ScanProjectView.ScannedProjects.Remove(project);
            DiaryEntries.RemoveAll(d => d.ProjectTitle == project.Title);

            ScanProjectView.LastDetectedTopicObjects.Clear();

            foreach (var starterTopic in ScanProjectView.StarterTopics)
            {
                ScanProjectView.LastDetectedTopicObjects.Add(new Topic
                {
                    TopicId = starterTopic.TopicId,
                    Name = starterTopic.Name,
                    Difficulty = starterTopic.Difficulty,
                    Status = "Not Started",
                    Score = 0
                });
            }

            var remainingTopics = DatabaseService.GetOverallTopics();

            foreach (var savedTopic in remainingTopics)
            {
                var matchingTopic = ScanProjectView.LastDetectedTopicObjects
                    .FirstOrDefault(t => t.Name == savedTopic.Name);

                if (matchingTopic != null)
                {
                    matchingTopic.Score = savedTopic.Score;
                    matchingTopic.Status = savedTopic.Status;
                }
            }

            var detectedOverallTopics = remainingTopics
                .Where(t => t.Status != "Not Started")
                .ToList();

            if (detectedOverallTopics.Count > 0)
            {
                double overallProgress = detectedOverallTopics.Average(t => t.Score);
                DatabaseService.SaveProgressRecord((int)Math.Round(overallProgress));
            }
            else
            {
                DatabaseService.SaveProgressRecord(0);
            }

            LoadProjectSelector();
            LoadEntries();
        }

        private void SaveEntryButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScanProjectView.LastDetectedTopicObjects.Count == 0)
            {
                MessageBox.Show("Please scan a project first before adding a diary entry.");
                return;
            }

            if (ProjectSelectorComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a project first.");
                return;
            }

            string entryText = DiaryInputTextBox.Text;
            if (string.IsNullOrWhiteSpace(entryText) ||
                entryText == DiaryInputTextBox.Tag.ToString())
            {
                MessageBox.Show("Please write a diary entry first.");
                return;
            }

            ProjectDiaryEntry entry = new ProjectDiaryEntry
            {
                EntryId = $"D{DiaryEntries.Count + 1:000}",
                ProjectTitle = ProjectSelectorComboBox.SelectedItem?.ToString() ?? "",
                Note = entryText,
                DateCreated = DateTime.Now
            };

            DiaryEntries.Add(entry);

            int projectId = DatabaseService.GetProjectIdByFilePath(
                ScanProjectView.ScannedProjects
                    .FirstOrDefault(p => p.Title == entry.ProjectTitle)?.FilePath ?? ""
            );

            if (projectId != -1)
                DatabaseService.SaveDiaryEntry(projectId, entry.Note);

            AddEntryOverlay.Visibility = Visibility.Collapsed;
            LoadEntries();
        }

        private void DiaryInputTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            DiaryInputTextBox.Text = DiaryInputTextBox.Tag.ToString();
            DiaryInputTextBox.Foreground = Brushes.Gray;
        }

        private void DiaryInputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (DiaryInputTextBox.Text == DiaryInputTextBox.Tag.ToString())
            {
                DiaryInputTextBox.Text = "";
                DiaryInputTextBox.Foreground = Brushes.White;
            }
        }

        private void DiaryInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DiaryInputTextBox.Text) ||
                DiaryInputTextBox.Text == DiaryInputTextBox.Tag.ToString())
            {
                DiaryInputTextBox.Text = DiaryInputTextBox.Tag.ToString();
                DiaryInputTextBox.Foreground = Brushes.Gray;
            }
        }

        public void LoadEntries()
        {
            DiaryEntries = DatabaseService.GetDiaryEntries();
            DiaryEntriesPanel.Children.Clear();

            int totalEntries = DiaryEntries.Count;
            int thisWeekEntries = DiaryEntries.Count(entry =>
                entry.DateCreated >= DateTime.Now.AddDays(-7));

            var mostActiveProject = DiaryEntries
                .GroupBy(entry => entry.ProjectTitle)
                .OrderByDescending(group => group.Count())
                .FirstOrDefault();

            if (ScanProjectView.ScannedProjects.Count == 0)
            {
                TotalEntriesText.Text = "0";
                ThisWeekEntriesText.Text = "0";
                MostActiveProjectText.Text = "No Project";
                MostActiveProjectCountText.Text = "0 entries";

                DiaryEntriesPanel.Children.Add(new TextBlock
                {
                    Text = "No scanned projects yet.",
                    Foreground = Brushes.White,
                    FontSize = 16
                });
                return;
            }

            TotalEntriesText.Text = totalEntries.ToString();
            ThisWeekEntriesText.Text = thisWeekEntries.ToString();

            if (mostActiveProject != null)
            {
                string projectName = mostActiveProject.Key;
                MostActiveProjectText.Text = projectName.Length > 30
                    ? projectName.Substring(0, 30) + "..."
                    : projectName;
                MostActiveProjectCountText.Text =
                    $"{mostActiveProject.Count()} entr{(mostActiveProject.Count() == 1 ? "y" : "ies")}";
            }
            else
            {
                MostActiveProjectText.Text = ScanProjectView.ScannedProjects.Last().Title;
                MostActiveProjectCountText.Text = "0 entries";
            }

            foreach (var project in ScanProjectView.ScannedProjects)
            {
                var projectEntries = DiaryEntries
                    .Where(entry => entry.ProjectTitle == project.Title)
                    .ToList();

                Border projectCard = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#11183A")),
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(18),
                    Margin = new Thickness(0, 0, 0, 14)
                };

                StackPanel projectContent = new StackPanel();

                // Header: info | dropdown
                Grid headerGrid = new Grid();
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

                StackPanel projectHeaderPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Top
                };

                Image folderIcon = new Image
                {
                    Source = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri("pack://application:,,,/Frontend/Assets/project.png")),
                    Width = 46,
                    Height = 46,
                    Margin = new Thickness(0, 2, 12, 0)
                };

                StackPanel projectInfoPanel = new StackPanel();

                projectInfoPanel.Children.Add(new TextBlock
                {
                    Text = project.Title,
                    Foreground = Brushes.White,
                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold
                });

                projectInfoPanel.Children.Add(new TextBlock
                {
                    Text = project.FilePath,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8F9BC7")),
                    FontSize = 12,
                    Margin = new Thickness(0, 2, 0, 6)
                });

                Border entryBadge = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#24167A")),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(10, 3, 10, 3),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                entryBadge.Child = new TextBlock
                {
                    Text = $"{projectEntries.Count} Entr{(projectEntries.Count == 1 ? "y" : "ies")}",
                    Foreground = Brushes.White,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold
                };

                projectInfoPanel.Children.Add(entryBadge);
                projectHeaderPanel.Children.Add(folderIcon);
                projectHeaderPanel.Children.Add(projectInfoPanel);

                Button dropdownButton = new Button
                {
                    Content = "▼",
                    Width = 30,
                    Height = 30,
                    Background = Brushes.Transparent,
                    Foreground = Brushes.White,
                    BorderBrush = Brushes.Transparent,
                    FontSize = 16,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Grid.SetColumn(projectHeaderPanel, 0);
                Grid.SetColumn(dropdownButton, 1);
                headerGrid.Children.Add(projectHeaderPanel);
                headerGrid.Children.Add(dropdownButton);
                projectContent.Children.Add(headerGrid);

                StackPanel entriesContainer = new StackPanel
                {
                    Visibility = Visibility.Collapsed,
                    Margin = new Thickness(0, 12, 0, 0)
                };

                dropdownButton.Click += (s, ev) =>
                {
                    if (entriesContainer.Visibility == Visibility.Collapsed)
                    {
                        entriesContainer.Visibility = Visibility.Visible;
                        dropdownButton.Content = "▲";
                    }
                    else
                    {
                        entriesContainer.Visibility = Visibility.Collapsed;
                        dropdownButton.Content = "▼";
                    }
                };

                if (projectEntries.Count == 0)
                {
                    entriesContainer.Children.Add(new TextBlock
                    {
                        Text = "No diary entries yet for this project.",
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7BB8")),
                        FontSize = 14,
                        Margin = new Thickness(0, 8, 0, 0)
                    });
                }
                else
                {
                    foreach (var entry in projectEntries)
                    {
                        Border entryCard = new Border
                        {
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#090E27")),
                            CornerRadius = new CornerRadius(12),
                            Padding = new Thickness(14),
                            Margin = new Thickness(0, 0, 0, 12)
                        };

                        StackPanel entryContent = new StackPanel();

                        Border idBadge = new Border
                        {
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5B3DF5")),
                            CornerRadius = new CornerRadius(10),
                            Padding = new Thickness(12, 5, 12, 5),
                            HorizontalAlignment = HorizontalAlignment.Left
                        };
                        idBadge.Child = new TextBlock
                        {
                            Text = entry.EntryId,
                            Foreground = Brushes.White,
                            FontWeight = FontWeights.Bold,
                            FontSize = 13
                        };

                        entryContent.Children.Add(idBadge);
                        entryContent.Children.Add(new TextBlock
                        {
                            Text = entry.Note,
                            Foreground = Brushes.White,
                            FontSize = 15,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 8, 0, 8)
                        });
                        entryContent.Children.Add(new TextBlock
                        {
                            Text = entry.DateCreated.ToString("MMM dd, yyyy • hh:mm tt"),
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B7C0DD")),
                            FontSize = 13
                        });

                        entryCard.Child = entryContent;
                        entriesContainer.Children.Add(entryCard);
                    }
                }

                projectContent.Children.Add(entriesContainer);
                projectCard.Child = projectContent;
                DiaryEntriesPanel.Children.Add(projectCard);
            }
        }

        private void ProjectSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // reserved for future use
        }
    }
}