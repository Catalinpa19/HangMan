using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HangMan.Services
{
    public class WordService
    {
        private readonly string _filePath = "Data/words.json";

        public Dictionary<string, List<string>> LoadWords()
        {
            if (!File.Exists(_filePath))
                return new Dictionary<string, List<string>>();

            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json)
                   ?? new Dictionary<string, List<string>>();
        }

        public List<string> GetAllWords(Dictionary<string, List<string>> categories)
        {
            return categories.Values.SelectMany(x => x).ToList();
        }
    }
}