using HangMan.Models;
using HangMan.ViewModels;
using System.Collections.Generic;
using System.Windows;

namespace HangMan
{
    public partial class SaveSelectionWindow : Window
    {
        public SavedGame? SelectedSave { get; private set; }

        public SaveSelectionWindow(List<SavedGame> saves)
        {
            InitializeComponent();

            SaveSelectionViewModel vm = new SaveSelectionViewModel(saves);
            vm.CloseAction = Close;
            DataContext = vm;

            Closed += (s, e) =>
            {
                SelectedSave = vm.SelectedSave;
            };
        }
    }
}