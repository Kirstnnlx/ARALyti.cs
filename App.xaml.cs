using ARALyti.cs.Data;
using ARALyti.cs.views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ARALyti.cs
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DatabaseService.InitializeDatabase();

            ScanProjectView.ScannedProjects = DatabaseService.GetProjects();

            base.OnStartup(e);

        }
    }
}