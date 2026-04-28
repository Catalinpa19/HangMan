using System.Diagnostics;
using System.Windows;

namespace HangMan
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("mailto:catalin.palade@student.unitbv.ro")
                {
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show("Nu s-a putut deschide clientul de email.");
            }
        }
    }
}