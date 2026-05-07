using ARALyti.cs.Data;
using System.Windows;
using System.Windows.Media;

namespace ARALyti.cs
{
    public partial class MainWindow : Window
    {
        private Brush activeBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5B3DF5"));
        private Brush inactiveBrush = Brushes.Transparent;

        public static string StudentName { get; private set; } = "";

        public MainWindow()
        {
            DatabaseService.InitializeDatabase();
            InitializeComponent();

            string savedName = DatabaseService.GetSavedUserName();

            if (!string.IsNullOrWhiteSpace(savedName))
            {
                StudentName = savedName;

                UpdateHelloTexts();

                LoginPage.Visibility = Visibility.Collapsed;
                AppShell.Visibility = Visibility.Visible;

                DashboardPanel.LoadDashboardData();
                ShowPanel("Dashboard");
            }
        }

        private void UpdateHelloTexts()
        {
            DashboardPanel.HelloUserText.Text = $"Hello, {StudentName}!";
            ScanProjectPanel.HelloUserText.Text = $"Hello, {StudentName}!";
            TopicsPanel.HelloUserText.Text = $"Hello, {StudentName}!";
            ProjectDiaryPanel.HelloUserText.Text = $"Hello, {StudentName}!";
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string name = LoginNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                LoginErrorText.Visibility = Visibility.Visible;
                return;
            }

            StudentName = name;

            DatabaseService.SaveUserName(StudentName);

            UpdateHelloTexts();

            LoginPage.Visibility = Visibility.Collapsed;
            AppShell.Visibility = Visibility.Visible;

            DashboardPanel.LoadDashboardData();
            ShowPanel("Dashboard");
        }

        public void ShowPanel(string panelName)
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