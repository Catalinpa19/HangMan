using HangMan.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HangMan.Services
{
    public class StatisticsService
    {
        private readonly string _filePath = "Data/statistics.json";

        public List<UserStatistics> LoadStatistics()
        {
            if (!File.Exists(_filePath))
                return new List<UserStatistics>();

            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<UserStatistics>>(json) ?? new List<UserStatistics>();
        }

        public void SaveStatistics(List<UserStatistics> statistics)
        {
            string? directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = JsonSerializer.Serialize(statistics, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_filePath, json);
        }

        public void RegisterResult(string username, string category, bool won)
        {
            List<UserStatistics> allStatistics = LoadStatistics();

            UserStatistics? userStats = allStatistics.FirstOrDefault(x => x.Username == username);
            if (userStats == null)
            {
                userStats = new UserStatistics { Username = username };
                allStatistics.Add(userStats);
            }

            if (!userStats.Categories.ContainsKey(category))
                userStats.Categories[category] = new CategoryStatistics();

            userStats.Categories[category].GamesPlayed++;

            if (won)
                userStats.Categories[category].GamesWon++;

            SaveStatistics(allStatistics);
        }

        public void DeleteUserStatistics(string username)
        {
            List<UserStatistics> allStatistics = LoadStatistics();
            UserStatistics? userStats = allStatistics.FirstOrDefault(x => x.Username == username);

            if (userStats != null)
            {
                allStatistics.Remove(userStats);
                SaveStatistics(allStatistics);
            }
        }
    }
}