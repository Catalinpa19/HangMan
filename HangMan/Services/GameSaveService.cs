using HangMan.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HangMan.Services
{
    public class GameSaveService
    {
        private readonly string _saveFolder = "Data/Saves";

        public void SaveGame(SavedGame game)
        {
            if (!Directory.Exists(_saveFolder))
                Directory.CreateDirectory(_saveFolder);

            string fileName = $"{game.UserName}_{game.SaveName}.json";
            string path = Path.Combine(_saveFolder, fileName);

            string json = JsonSerializer.Serialize(game, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
        }

        public List<SavedGame> LoadSavedGamesForUser(string username)
        {
            if (!Directory.Exists(_saveFolder))
                Directory.CreateDirectory(_saveFolder);

            List<SavedGame> games = new();

            foreach (string file in Directory.GetFiles(_saveFolder, $"{username}_*.json"))
            {
                string json = File.ReadAllText(file);
                SavedGame? game = JsonSerializer.Deserialize<SavedGame>(json);
                if (game != null)
                    games.Add(game);
            }

            return games.OrderBy(x => x.SaveName).ToList();
        }

        public void DeleteSavedGamesForUser(string username)
        {
            if (!Directory.Exists(_saveFolder))
                return;

            foreach (string file in Directory.GetFiles(_saveFolder, $"{username}_*.json"))
                File.Delete(file);
        }
    }
}