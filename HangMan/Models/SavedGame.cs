using System.Collections.Generic;

namespace HangMan.Models
{
    public class SavedGame
    {
        public string UserName { get; set; } = string.Empty;
        public string SaveName { get; set; } = string.Empty;
        public string Category { get; set; } = "All Categories";
        public string CurrentWord { get; set; } = string.Empty;
        public List<string> GuessedLetters { get; set; } = new();
        public int WrongGuesses { get; set; }
        public int SecondsLeft { get; set; }
        public int CurrentLevel { get; set; }
    }
}