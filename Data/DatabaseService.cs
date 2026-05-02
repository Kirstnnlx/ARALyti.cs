using Microsoft.Data.Sqlite;
using System.IO;
using System.Collections.Generic;

namespace ARALyti.cs.Data
{
    public static class DatabaseService
    {
        private static readonly string DatabasePath = "aralytics.db";
        private static readonly string ConnectionString = $"Data Source={DatabasePath}";

        public static void InitializeDatabase()
        {
            using SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string createTablesQuery = @"
                CREATE TABLE IF NOT EXISTS UserProfile (
                    UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Streak INTEGER NOT NULL,
                    LastOpenedDate TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Projects (
                    ProjectId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    FilePath TEXT NOT NULL UNIQUE,
                    Status TEXT NOT NULL,
                    DateScanned TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Topics (
                    TopicId INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProjectId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    Difficulty TEXT NOT NULL,
                    Status TEXT NOT NULL,
                    Score INTEGER NOT NULL,
                    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId)
                    UNIQUE(ProjectId, Name)
                );

                CREATE TABLE IF NOT EXISTS ProjectDiaryEntries (
                    EntryId INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProjectId INTEGER NOT NULL,
                    Note TEXT NOT NULL,
                    DateCreated TEXT NOT NULL,
                    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId)
                );
            ";

            using SqliteCommand command = new SqliteCommand(createTablesQuery, connection);
            command.ExecuteNonQuery();
        }

        public static void SaveProject(string title, string filePath)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = @"
                INSERT OR IGNORE INTO Projects (Title, FilePath, Status, DateScanned)
                VALUES (@title, @filePath, 'Scanned', @date);
            ";

            using var command = new SqliteCommand(query, connection);

            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@filePath", filePath);
            command.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd"));

            command.ExecuteNonQuery();
        }

        public static int GetProjectIdByFilePath(string filePath)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = "SELECT ProjectId FROM Projects WHERE FilePath = @filePath";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@filePath", filePath);

            object? result = command.ExecuteScalar(); // 👈 add ?

            if (result == null)
                return -1;

            return Convert.ToInt32(result);
        }

        public static void SaveTopic(int projectId, string name, string difficulty, string status, int score)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = @"
                INSERT INTO Topics (ProjectId, Name, Difficulty, Status, Score)
                VALUES (@projectId, @name, @difficulty, @status, @score)
                ON CONFLICT(ProjectId, Name)
                DO UPDATE SET
                    Difficulty = excluded.Difficulty,
                    Status = excluded.Status,
                    Score = excluded.Score;
            ";

            using var command = new SqliteCommand(query, connection);

            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@difficulty", difficulty);
            command.Parameters.AddWithValue("@status", status);
            command.Parameters.AddWithValue("@score", score);

            command.ExecuteNonQuery();
        }

        public static void SaveDiaryEntry(int projectId, string note)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = @"
                INSERT INTO ProjectDiaryEntries (ProjectId, Note, DateCreated)
                VALUES (@projectId, @note, @dateCreated);
            ";

            using var command = new SqliteCommand(query, connection);

            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@note", note);
            command.Parameters.AddWithValue("@dateCreated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        }

        public static List<ARALyti.cs.Models.Project> GetProjects()
        {
            List<ARALyti.cs.Models.Project> projects = new List<ARALyti.cs.Models.Project>();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = "SELECT ProjectId, Title, FilePath, Status FROM Projects";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                projects.Add(new ARALyti.cs.Models.Project
                {
                    ProjectId = reader["ProjectId"].ToString() ?? "",
                    Title = reader["Title"].ToString() ?? "",
                    FilePath = reader["FilePath"].ToString() ?? "",
                    Status = reader["Status"].ToString() ?? ""
                });
            }

            return projects;
        }

    }
}
