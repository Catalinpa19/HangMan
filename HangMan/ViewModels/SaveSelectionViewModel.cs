using HangMan.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HangMan.ViewModels
{
    public class SaveSelectionViewModel : BaseViewModel
    {
        public ObservableCollection<SavedGame> Saves { get; set; }

        private SavedGame? _selectedSave;
        public SavedGame? SelectedSave
        {
            get => _selectedSave;
            set
            {
                _selectedSave = value;
                OnPropertyChanged();
                SelectCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand SelectCommand { get; }
        public RelayCommand CancelCommand { get; }

        public Action? CloseAction { get; set; }

        public SaveSelectionViewModel(List<SavedGame> saves)
        {
            Saves = new ObservableCollection<SavedGame>(saves);

            SelectCommand = new RelayCommand(Select, CanSelect);
            CancelCommand = new RelayCommand(Cancel);
        }

        private bool CanSelect()
        {
            return SelectedSave != null;
        }

        private void Select()
        {
            CloseAction?.Invoke();
        }

        private void Cancel()
        {
            SelectedSave = null;
            CloseAction?.Invoke();
        }
    }
}