using ARALyti.cs.Data;
using ARALyti.cs.views;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Linq;

namespace ARALyti.cs
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DatabaseService.InitializeDatabase();

            ScanProjectView.ScannedProjects = DatabaseService.GetProjects();

            if (ScanProjectView.ScannedProjects.Count > 0)
            {
                var latestProject = ScanProjectView.ScannedProjects.Last();

                int projectId = DatabaseService.GetProjectIdByFilePath(latestProject.FilePath);

                if (projectId != -1)
                {
                    ScanProjectView.LastDetectedTopicObjects =
                        DatabaseService.GetTopicsByProjectId(projectId);
                }
            }

            base.OnStartup(e);

        }
    }
}