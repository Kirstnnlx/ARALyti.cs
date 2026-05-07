using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ARALyti.cs
{
    public partial class LoginWindow : Window
    {
        public static string StudentName { get; private set; } = "";
        private const string PlaceholderText = "Enter your name";

        public LoginWindow()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.6));
            this.BeginAnimation(OpacityProperty, fadeIn);

            SetPlaceholder();

            string[] quotes = {
                "“The future belongs to those who believe in the beauty of their dreams.” – Eleanor Roosevelt",
                "“Learning is not attained by chance, it must be sought with ardor.” – Abigail Adams",
                "“The expert in anything was once a beginner.” – Helen Hayes",
                "“Education is the most powerful weapon which you can use to change the world.” – Mandela"
            };
            Random rand = new Random();
            txtQuote.Text = quotes[rand.Next(quotes.Length)];

            txtName.GotFocus += TxtName_GotFocus;
            txtName.LostFocus += TxtName_LostFocus;
            btnStart.Click += BtnStart_Click;
        }

        private void SetPlaceholder()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                txtName.Text = PlaceholderText;
                txtName.Foreground = (Brush)FindResource("PlaceholderBrush");
            }
        }

        private void TxtName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtName.Text == PlaceholderText)
            {
                txtName.Text = "";
                txtName.Foreground = Brushes.White;
                lblError.Visibility = Visibility.Collapsed;
            }
        }

        private void TxtName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                txtName.Text = PlaceholderText;
                txtName.Foreground = (Brush)FindResource("PlaceholderBrush");
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();

            if (string.IsNullOrEmpty(name) || name == PlaceholderText)
            {
                lblError.Content = "Please enter your name.";
                lblError.Visibility = Visibility.Visible;
                return;
            }

            StudentName = name;


            MainWindow main = new MainWindow();
            main.Show();

            this.Close();
        }
    }
}