using HangMan.ViewModels;
using System.Windows;

namespace HangMan
{
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow()
        {
            InitializeComponent();
            DataContext = new StatisticsViewModel();
        }
    }
}