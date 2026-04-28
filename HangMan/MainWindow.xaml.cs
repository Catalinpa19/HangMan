using System.Windows;
using HangMan.ViewModels;

namespace HangMan
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new StartViewModel();
        }
    }
}