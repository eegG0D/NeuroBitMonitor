using System.Windows;
using NeuroBitMonitor.ViewModels;

namespace NeuroBitMonitor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Set the DataContext directly (optional, as XAML does it too)
            // This ensures the ViewModel is ready when the window loads
            // this.DataContext = new MainViewModel(); 
        }
    }
}