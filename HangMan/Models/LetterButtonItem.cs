using HangMan.ViewModels;

namespace HangMan.Models
{
    public class LetterButtonItem : BaseViewModel
    {
        public string Letter { get; set; } = string.Empty;

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }
    }
}