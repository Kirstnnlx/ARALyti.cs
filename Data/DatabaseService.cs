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
                    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId),
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

        public static List<ARALyti.cs.Models.Topic> GetTopicsByProjectId(int projectId)
        {
            List<ARALyti.cs.Models.Topic> topics = new List<ARALyti.cs.Models.Topic>();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = @"
                SELECT Name, Difficulty, Status, Score
                FROM Topics
                WHERE ProjectId = @projectId
            ";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@projectId", projectId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                topics.Add(new ARALyti.cs.Models.Topic
                {
                    Name = reader["Name"].ToString() ?? "",
                    Difficulty = reader["Difficulty"].ToString() ?? "",
                    Status = reader["Status"].ToString() ?? "",
                    Score = Convert.ToInt32(reader["Score"])
                });
            }

            return topics;
        }

        public static void DeleteProjectByFilePath(string filePath)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            int projectId = GetProjectIdByFilePath(filePath);

            if (projectId == -1)
                return;

            string deleteTopicsQuery = "DELETE FROM Topics WHERE ProjectId = @projectId";
            using var deleteTopicsCommand = new SqliteCommand(deleteTopicsQuery, connection);
            deleteTopicsCommand.Parameters.AddWithValue("@projectId", projectId);
            deleteTopicsCommand.ExecuteNonQuery();

            string deleteDiaryQuery = "DELETE FROM ProjectDiaryEntries WHERE ProjectId = @projectId";
            using var deleteDiaryCommand = new SqliteCommand(deleteDiaryQuery, connection);
            deleteDiaryCommand.Parameters.AddWithValue("@projectId", projectId);
            deleteDiaryCommand.ExecuteNonQuery();

            string deleteProjectQuery = "DELETE FROM Projects WHERE ProjectId = @projectId";
            using var deleteProjectCommand = new SqliteCommand(deleteProjectQuery, connection);
            deleteProjectCommand.Parameters.AddWithValue("@projectId", projectId);
            deleteProjectCommand.ExecuteNonQuery();
        }

        public static List<ARALyti.cs.Models.Topic> GetOverallTopics()
        {
            var topics = new List<ARALyti.cs.Models.Topic>();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = @"
                SELECT Name, Difficulty, AVG(Score) AS AverageScore
                FROM Topics
                WHERE Status != 'Not Started'
                GROUP BY Name, Difficulty;
            ";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                int averageScore = Convert.ToInt32(reader["AverageScore"]);

                string status = "Developing";
                if (averageScore >= 80)
                    status = "Strong";
                else if (averageScore < 40)
                    status = "Weak";

                topics.Add(new ARALyti.cs.Models.Topic
                {
                    Name = reader["Name"].ToString() ?? "",
                    Difficulty = reader["Difficulty"].ToString() ?? "",
                    Status = status,
                    Score = averageScore
                });
            }

            return topics;
        }

    }
}
