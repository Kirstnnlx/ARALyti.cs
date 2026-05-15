using System.Windows;
using ARALyti.cs.Data;
using ARALyti.cs.views;

namespace ARALyti.cs
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Ensure database tables exist
            DatabaseService.InitializeDatabase();

            // Load all scanned projects from database into static list
            ScanProjectView.ScannedProjects = DatabaseService.GetProjects();

            // If there are existing projects, load the latest one's topics
            if (ScanProjectView.ScannedProjects != null && ScanProjectView.ScannedProjects.Count > 0)
            {
                var latestProject = ScanProjectView.ScannedProjects.Last();

                if (latestProject != null && !string.IsNullOrEmpty(latestProject.FilePath))
                {
                    int projectId = DatabaseService.GetProjectIdByFilePath(latestProject.FilePath);

                    if (projectId != -1)
                    {
                        var topics = DatabaseService.GetTopicsByProjectId(projectId);
                        if (topics != null)
                        {
                            ScanProjectView.LastDetectedTopicObjects = topics;
                        }
                    }
                }
            }

            base.OnStartup(e);
        }
    }
}