using System;

namespace ARALyti.cs.Models
{
    public class ProjectDiaryEntry
    {
        public string EntryId { get; set; } = "";
        public string ProjectTitle { get; set; } = "";
        public string Note { get; set; } = "";
        public DateTime DateCreated { get; set; }
    }
}