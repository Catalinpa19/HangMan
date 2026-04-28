using HangMan.Models;
using HangMan.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace HangMan.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        private readonly User _currentUser;
        private readonly WordService _wordService;
        private readonly GameSaveService _gameSaveService;
        private readonly StatisticsService _statisticsService;
        private readonly DispatcherTimer _timer;
        private readonly Random _random = new Random();

        private Dictionary<string, List<string>> _categories = new();
        private string _selectedCategory = "All Categories";
        private string _currentWord = string.Empty;
        private HashSet<char> _guessedLetters = new HashSet<char>();
        private int _wrongGuesses;
        private int _secondsLeft = 120;
        private int _currentLevel;
        private bool _gameFinished;

        public ObservableCollection<LetterButtonItem> Letters { get; set; } = new();
        public ObservableCollection<string> CategoryItems { get; set; } = new();

        public string CurrentUsername => _currentUser.Username;

        private BitmapImage? _currentUserImage;
        public BitmapImage? CurrentUserImage
        {
            get => _currentUserImage;
            set
            {
                _currentUserImage = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage? _hangmanImage;
        public BitmapImage? HangmanImage
        {
            get => _hangmanImage;
            set
            {
                _hangmanImage = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCategoryDisplay => $"Category: {_selectedCategory}";
        public string LevelDisplay => $"Level: {_currentLevel}";
        public string TimerDisplay => $"Time left: {_secondsLeft}";
        public string WrongGuessesDisplay => $"Wrong guesses: {_wrongGuesses}/6";

        public string CurrentWordDisplay
        {
            get
            {
                return string.Join(" ", _currentWord.Select(c => _guessedLetters.Contains(c) ? c : '_'));
            }
        }

        private string _statusMessage = "Press New Game";
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand NewGameCommand { get; }
        public RelayCommand SaveGameCommand { get; }
        public RelayCommand OpenGameCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand StatisticsCommand { get; }
        public RelayCommand AboutCommand { get; }

        public RelayCommandWithParameter GuessLetterCommand { get; }
        public RelayCommandWithParameter ChangeCategoryCommand { get; }

        public Action? CloseAction { get; set; }

        public GameViewModel(User user)
        {
            _currentUser = user;
            _wordService = new WordService();
            _gameSaveService = new GameSaveService();
            _statisticsService = new StatisticsService();

            _categories = _wordService.LoadWords();

            CategoryItems.Add("All Categories");
            foreach (string category in _categories.Keys)
                CategoryItems.Add(category);

            foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
                Letters.Add(new LetterButtonItem { Letter = c.ToString(), IsEnabled = true });

            LoadUserImage();
            UpdateHangmanImage();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            NewGameCommand = new RelayCommand(NewGame);
            SaveGameCommand = new RelayCommand(SaveGame);
            OpenGameCommand = new RelayCommand(OpenGame);
            CancelCommand = new RelayCommand(Cancel);
            StatisticsCommand = new RelayCommand(OpenStatistics);
            AboutCommand = new RelayCommand(OpenAbout);

            GuessLetterCommand = new RelayCommandWithParameter(GuessLetter);
            ChangeCategoryCommand = new RelayCommandWithParameter(ChangeCategory);
        }

        private void LoadUserImage()
        {
            if (string.IsNullOrWhiteSpace(_currentUser.ImagePath))
                return;

            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _currentUser.ImagePath);
            if (!File.Exists(fullPath))
                return;

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(fullPath, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();

            CurrentUserImage = image;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_gameFinished)
                return;

            _secondsLeft--;
            OnPropertyChanged(nameof(TimerDisplay));

            if (_secondsLeft <= 0)
            {
                _timer.Stop();
                LoseGame("Time is up!");
            }
        }

        private void NewGame()
        {
            _currentLevel = 0;
            OnPropertyChanged(nameof(LevelDisplay));
            StartRound();
        }

        private void ChangeCategory(object? parameter)
        {
            _selectedCategory = parameter?.ToString() ?? "All Categories";

            _currentLevel = 0;
            OnPropertyChanged(nameof(LevelDisplay));
            OnPropertyChanged(nameof(SelectedCategoryDisplay));

            StartRound();
        }

        private void StartRound()
        {
            List<string> words = new();

            if (_categories.Count == 0)
            {
                StatusMessage = "No words file found.";
                return;
            }

            if (_selectedCategory == "All Categories")
                words = _wordService.GetAllWords(_categories);
            else if (_categories.ContainsKey(_selectedCategory))
                words = _categories[_selectedCategory];

            if (words.Count == 0)
            {
                StatusMessage = "No words available.";
                return;
            }

            _currentWord = words[_random.Next(words.Count)].ToUpperInvariant();
            _guessedLetters.Clear();
            _wrongGuesses = 0;
            _secondsLeft = 120;
            _gameFinished = false;

            foreach (LetterButtonItem item in Letters)
                item.IsEnabled = true;

            OnPropertyChanged(nameof(CurrentWordDisplay));
            OnPropertyChanged(nameof(WrongGuessesDisplay));
            OnPropertyChanged(nameof(TimerDisplay));

            UpdateHangmanImage();
            StatusMessage = "Choose a letter";

            _timer.Stop();
            _timer.Start();
        }

        private void GuessLetter(object? parameter)
        {
            if (_gameFinished || parameter == null || string.IsNullOrWhiteSpace(_currentWord))
                return;

            char letter = parameter.ToString()![0];

            LetterButtonItem? button = Letters.FirstOrDefault(x => x.Letter == letter.ToString());
            if (button == null || !button.IsEnabled)
                return;

            button.IsEnabled = false;

            _secondsLeft = 120;
            OnPropertyChanged(nameof(TimerDisplay));

            _guessedLetters.Add(letter);

            OnPropertyChanged(nameof(CurrentWordDisplay));

            if (_currentWord.Contains(letter))
            {
                StatusMessage = $"Good! Letter {letter} exists.";

                if (_currentWord.All(c => _guessedLetters.Contains(c)))
                    WinRound();
            }
            else
            {
                _wrongGuesses++;
                OnPropertyChanged(nameof(WrongGuessesDisplay));
                UpdateHangmanImage();
                StatusMessage = $"Wrong! Letter {letter} does not exist.";

                if (_wrongGuesses >= 6)
                    LoseGame("You lost this round!");
            }
        }

        private void WinRound()
        {
            _timer.Stop();
            _currentLevel++;
            OnPropertyChanged(nameof(LevelDisplay));

            if (_currentLevel >= 3)
            {
                _gameFinished = true;
                StatusMessage = "You won the game! 3 levels completed.";
                _statisticsService.RegisterResult(_currentUser.Username, _selectedCategory, true);
                return;
            }

            MessageBox.Show("Round won! Next level starts now.");
            StartRound();
        }

        private void LoseGame(string message)
        {
            _gameFinished = true;
            _timer.Stop();
            _currentLevel = 0;
            OnPropertyChanged(nameof(LevelDisplay));
            StatusMessage = message;
            _statisticsService.RegisterResult(_currentUser.Username, _selectedCategory, false);
        }

        private void UpdateHangmanImage()
        {
            string relativePath = Path.Combine("Data", "Hangman", $"h{_wrongGuesses}.png");
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

            if (!File.Exists(fullPath))
            {
                HangmanImage = null;
                return;
            }

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(fullPath, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();

            HangmanImage = image;
        }

        private void SaveGame()
        {
            if (string.IsNullOrWhiteSpace(_currentWord))
                return;

            string saveName = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            SavedGame save = new SavedGame
            {
                UserName = _currentUser.Username,
                SaveName = saveName,
                Category = _selectedCategory,
                CurrentWord = _currentWord,
                GuessedLetters = _guessedLetters.Select(x => x.ToString()).ToList(),
                WrongGuesses = _wrongGuesses,
                SecondsLeft = _secondsLeft,
                CurrentLevel = _currentLevel
            };

            _gameSaveService.SaveGame(save);
            StatusMessage = $"Game saved: {saveName}";
        }

        private void OpenGame()
        {
            List<SavedGame> saves = _gameSaveService.LoadSavedGamesForUser(_currentUser.Username);

            if (saves.Count == 0)
            {
                StatusMessage = "No saved games found.";
                return;
            }

            SaveSelectionWindow window = new SaveSelectionWindow(saves);
            window.ShowDialog();

            if (window.SelectedSave == null)
            {
                StatusMessage = "No save selected.";
                return;
            }

            SavedGame save = window.SelectedSave;

            _selectedCategory = save.Category;
            _currentWord = save.CurrentWord;
            _guessedLetters = save.GuessedLetters.Select(x => x[0]).ToHashSet();
            _wrongGuesses = save.WrongGuesses;
            _secondsLeft = save.SecondsLeft;
            _currentLevel = save.CurrentLevel;
            _gameFinished = false;

            foreach (LetterButtonItem item in Letters)
                item.IsEnabled = !_guessedLetters.Contains(item.Letter[0]);

            OnPropertyChanged(nameof(CurrentWordDisplay));
            OnPropertyChanged(nameof(WrongGuessesDisplay));
            OnPropertyChanged(nameof(TimerDisplay));
            OnPropertyChanged(nameof(LevelDisplay));
            OnPropertyChanged(nameof(SelectedCategoryDisplay));

            UpdateHangmanImage();
            StatusMessage = $"Loaded game: {save.SaveName}";

            _timer.Stop();
            _timer.Start();
        }

        private void Cancel()
        {
            _timer.Stop();
            CloseAction?.Invoke();
        }

        private void OpenStatistics()
        {
            StatisticsWindow window = new StatisticsWindow();
            window.ShowDialog();
        }

        private void OpenAbout()
        {
            AboutWindow window = new AboutWindow();
            window.ShowDialog();
        }

        public void HandleKey(Key key)
        {
            if (key >= Key.A && key <= Key.Z)
            {
                char letter = key.ToString()[0];
                GuessLetter(letter.ToString());
            }
        }
    }
}