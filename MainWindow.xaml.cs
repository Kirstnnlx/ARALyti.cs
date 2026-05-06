using System.Windows;
using System.Windows.Media;

namespace ARALyti.cs
{
    public partial class MainWindow : Window
    {
        private Brush activeBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5B3DF5"));
        private Brush inactiveBrush = Brushes.Transparent;

        public MainWindow()
        {
            InitializeComponent();
            ShowPanel("Dashboard");
        }

        private void ShowPanel(string panelName)
        {
            DashboardPanel.Visibility = Visibility.Collapsed;
            ScanProjectPanel.Visibility = Visibility.Collapsed;
            TopicsPanel.Visibility = Visibility.Collapsed;
            ProjectDiaryPanel.Visibility = Visibility.Collapsed;

            ResetSidebarButtons();

            switch (panelName)
            {
                case "Dashboard":
                    DashboardPanel.Visibility = Visibility.Visible;
                    DashboardButton.Background = activeBrush;
                    break;

                case "ScanProject":
                    ScanProjectPanel.Visibility = Visibility.Visible;
                    ScanProjectButton.Background = activeBrush;
                    break;

                case "Topics":
                    TopicsPanel.Visibility = Visibility.Visible;
                    TopicsButton.Background = activeBrush;
                    break;

                case "ProjectDiary":
                    ProjectDiaryPanel.Visibility = Visibility.Visible;
                    ProjectDiaryButton.Background = activeBrush;
                    break;
            }
        }

        private void ResetSidebarButtons()
        {
            DashboardButton.Background = inactiveBrush;
            ScanProjectButton.Background = inactiveBrush;
            TopicsButton.Background = inactiveBrush;
            ProjectDiaryButton.Background = inactiveBrush;
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            DashboardPanel.LoadDashboardData();
            ShowPanel("Dashboard");
        }

        private void ScanProjectButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel("ScanProject");
        }

        private void TopicsButton_Click(object sender, RoutedEventArgs e)
        {
            TopicsPanel.LoadTopics();
            ShowPanel("Topics");
        }

        private void ProjectDiaryButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectDiaryPanel.LoadProjectSelector();
            ProjectDiaryPanel.LoadEntries();
            ShowPanel("ProjectDiary");
        }
    }
}