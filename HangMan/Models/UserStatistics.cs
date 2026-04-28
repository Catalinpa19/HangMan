using System.Collections.Generic;

namespace HangMan.Models
{
    public class UserStatistics
    {
        public string Username { get; set; } = string.Empty;
        public Dictionary<string, CategoryStatistics> Categories { get; set; } = new();
    }
}