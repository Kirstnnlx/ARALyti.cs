using ARALyti.cs.Data;
using ARALyti.cs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

        public void LoadProjectSelector()
        {
            ProjectSelectorComboBox.Items.Clear();

            foreach (var project in ScanProjectView.ScannedProjects)
            {
                ProjectSelectorComboBox.Items.Add(project.Title);
            }

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

            if (string.IsNullOrWhiteSpace(DiaryInputTextBox.Text))
            {
                MessageBox.Show("Please write a diary entry first.");
                return;
            }

            ProjectDiaryEntry entry = new ProjectDiaryEntry
            {
                EntryId = $"D{DiaryEntries.Count + 1:000}",
                ProjectTitle = ProjectSelectorComboBox.SelectedItem?.ToString() ?? "",
                Note = DiaryInputTextBox.Text,
                DateCreated = DateTime.Now
            };

            DiaryEntries.Add(entry);

            int projectId = DatabaseService.GetProjectIdByFilePath(
                ScanProjectView.ScannedProjects
                    .FirstOrDefault(p => p.Title == entry.ProjectTitle)?.FilePath ?? ""
            );

            if (projectId != -1)
            {
                DatabaseService.SaveDiaryEntry(projectId, entry.Note);
            }

            DiaryInputTextBox.Text = "";
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

                TextBlock emptyText = new TextBlock
                {
                    Text = "No scanned projects yet.",
                    Foreground = Brushes.White,
                    FontSize = 16
                };

                DiaryEntriesPanel.Children.Add(emptyText);
                return;
            }

            TotalEntriesText.Text = totalEntries.ToString();
            ThisWeekEntriesText.Text = thisWeekEntries.ToString();

            if (mostActiveProject != null)
            {
                string projectName = mostActiveProject.Key;

                string displayName = projectName.Length > 30
                    ? projectName.Substring(0, 30) + "..."
                    : projectName;

                MostActiveProjectText.Text = displayName;

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
                Border projectCard = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#11183A")),
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(18),
                    Margin = new Thickness(0, 0, 0, 14)
                };

                StackPanel projectContent = new StackPanel();

                Grid headerGrid = new Grid();
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition());
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

                // Project title
                TextBlock projectTitleText = new TextBlock
                {
                    Text = project.Title,
                    Foreground = Brushes.White,
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Delete button
                Button deleteButton = new Button
                {
                    Content = "🗑",
                    Width = 30,
                    Height = 30,
                    Background = Brushes.Transparent,
                    Foreground = Brushes.White,
                    BorderBrush = Brushes.Transparent,
                    FontSize = 16,
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                // Click event
                deleteButton.Click += (s, e) =>
                {
                    var result = MessageBox.Show(
                        "Delete this project and all its data?",
                        "Confirm Delete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        DatabaseService.DeleteProjectByFilePath(project.FilePath);

                        // remove from memory
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

                        // refresh UI
                        LoadProjectSelector();
                        LoadEntries();
                    }
                };

                Grid.SetColumn(projectTitleText, 0);
                Grid.SetColumn(deleteButton, 1);

                headerGrid.Children.Add(projectTitleText);
                headerGrid.Children.Add(deleteButton);

                TextBlock projectPathText = new TextBlock
                {
                    Text = project.FilePath,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B7C0DD")),
                    FontSize = 14,
                    Margin = new Thickness(0, 6, 0, 0)
                };

                var projectEntries = DiaryEntries
                    .Where(entry => entry.ProjectTitle == project.Title)
                    .ToList();

                TextBlock entryCountText = new TextBlock
                {
                    Text = $"{projectEntries.Count} entr{(projectEntries.Count == 1 ? "y" : "ies")}",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B7C0DD")),
                    FontSize = 14,
                    Margin = new Thickness(0, 6, 0, 12)
                };

                projectContent.Children.Add(headerGrid);
                projectContent.Children.Add(projectPathText);
                projectContent.Children.Add(entryCountText);

                if (projectEntries.Count == 0)
                {
                    TextBlock noEntriesText = new TextBlock
                    {
                        Text = "No diary entries yet for this project.",
                        Foreground = Brushes.White,
                        FontSize = 15,
                        Margin = new Thickness(0, 8, 0, 0)
                    };

                    projectContent.Children.Add(noEntriesText);
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

                        TextBlock idText = new TextBlock
                        {
                            Text = entry.EntryId,
                            Foreground = Brushes.White,
                            FontWeight = FontWeights.Bold,
                            FontSize = 13
                        };

                        idBadge.Child = idText;

                        TextBlock noteText = new TextBlock
                        {
                            Text = entry.Note,
                            Foreground = Brushes.White,
                            FontSize = 15,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 8, 0, 8)
                        };

                        TextBlock dateText = new TextBlock
                        {
                            Text = entry.DateCreated.ToString("MMM dd, yyyy • hh:mm tt"),
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B7C0DD")),
                            FontSize = 13
                        };

                        entryContent.Children.Add(idBadge);
                        entryContent.Children.Add(noteText);
                        entryContent.Children.Add(dateText);

                        entryCard.Child = entryContent;
                        projectContent.Children.Add(entryCard);
                    }
                }

                projectCard.Child = projectContent;
                DiaryEntriesPanel.Children.Add(projectCard);
            }
        }

        private void ProjectSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProjectSelectorPlaceholder.Visibility =
                ProjectSelectorComboBox.SelectedItem == null
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }
    }
}