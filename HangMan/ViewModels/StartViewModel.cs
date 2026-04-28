using HangMan.Models;
using HangMan.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace HangMan.ViewModels
{
    public class StartViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private readonly GameSaveService _gameSaveService;
        private readonly StatisticsService _statisticsService;

        public ObservableCollection<User> Users { get; set; }

        private User? _selectedUser;
        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();

                LoadSelectedUserImage();

                DeleteUserCommand.RaiseCanExecuteChanged();
                PlayCommand.RaiseCanExecuteChanged();
            }
        }

        private BitmapImage? _selectedUserImage;
        public BitmapImage? SelectedUserImage
        {
            get => _selectedUserImage;
            set
            {
                _selectedUserImage = value;
                OnPropertyChanged();
            }
        }

        private string _newUsername = string.Empty;
        public string NewUsername
        {
            get => _newUsername;
            set
            {
                _newUsername = value;
                OnPropertyChanged();
                AddUserCommand.RaiseCanExecuteChanged();
            }
        }

        private string _newImagePath = string.Empty;
        public string NewImagePath
        {
            get => _newImagePath;
            set
            {
                _newImagePath = value;
                OnPropertyChanged();
                AddUserCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand ChooseImageCommand { get; set; }
        public RelayCommand AddUserCommand { get; set; }
        public RelayCommand DeleteUserCommand { get; set; }
        public RelayCommand PlayCommand { get; set; }

        public StartViewModel()
        {
            _userService = new UserService();
            _gameSaveService = new GameSaveService();
            _statisticsService = new StatisticsService();

            Users = new ObservableCollection<User>(_userService.LoadUsers());

            ChooseImageCommand = new RelayCommand(ChooseImage);
            AddUserCommand = new RelayCommand(AddUser, CanAddUser);
            DeleteUserCommand = new RelayCommand(DeleteUser, CanUseSelectedUser);
            PlayCommand = new RelayCommand(Play, CanUseSelectedUser);
        }

        private void LoadSelectedUserImage()
        {
            if (SelectedUser == null || string.IsNullOrWhiteSpace(SelectedUser.ImagePath))
            {
                SelectedUserImage = null;
                return;
            }

            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SelectedUser.ImagePath);

                if (!File.Exists(fullPath))
                {
                    SelectedUserImage = null;
                    return;
                }

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(fullPath, UriKind.Absolute);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();

                SelectedUserImage = image;
            }
            catch
            {
                SelectedUserImage = null;
            }
        }

        private void ChooseImage()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif";

            if (dialog.ShowDialog() == true)
            {
                string selectedFile = dialog.FileName;
                string fileName = Path.GetFileName(selectedFile);

                string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Images");
                Directory.CreateDirectory(imagesFolder);

                string destinationPath = Path.Combine(imagesFolder, fileName);
                File.Copy(selectedFile, destinationPath, true);

                NewImagePath = Path.Combine("Data", "Images", fileName);
            }
        }

        private bool CanAddUser()
        {
            return !string.IsNullOrWhiteSpace(NewUsername)
                   && !string.IsNullOrWhiteSpace(NewImagePath);
        }

        private void AddUser()
        {
            bool userAlreadyExists = Users.Any(u =>
                u.Username.Equals(NewUsername, StringComparison.OrdinalIgnoreCase));

            if (userAlreadyExists)
                return;

            User newUser = new User
            {
                Username = NewUsername,
                ImagePath = NewImagePath
            };

            Users.Add(newUser);
            _userService.SaveUsers(Users.ToList());

            NewUsername = string.Empty;
            NewImagePath = string.Empty;
        }

        private void DeleteUser()
        {
            if (SelectedUser == null)
                return;

            string username = SelectedUser.Username;
            string imagePath = SelectedUser.ImagePath;

            Users.Remove(SelectedUser);
            _userService.SaveUsers(Users.ToList());

            _gameSaveService.DeleteSavedGamesForUser(username);
            _statisticsService.DeleteUserStatistics(username);

            string fullImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);
            bool imageStillUsed = Users.Any(u => u.ImagePath == imagePath);

            if (!imageStillUsed && File.Exists(fullImagePath))
                File.Delete(fullImagePath);

            SelectedUser = null;
            SelectedUserImage = null;
        }

        private bool CanUseSelectedUser()
        {
            return SelectedUser != null;
        }

        private void Play()
        {
            if (SelectedUser == null)
                return;

            GameWindow gameWindow = new GameWindow(SelectedUser);
            gameWindow.Show();
        }
    }
}