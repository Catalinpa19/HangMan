using HangMan.Models;
using HangMan.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace HangMan
{
    public partial class GameWindow : Window
    {
        public GameWindow(User user)
        {
            InitializeComponent();
            GameViewModel vm = new GameViewModel(user);
            vm.CloseAction = Close;
            DataContext = vm;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is GameViewModel vm)
                vm.HandleKey(e.Key);
        }
    }
}