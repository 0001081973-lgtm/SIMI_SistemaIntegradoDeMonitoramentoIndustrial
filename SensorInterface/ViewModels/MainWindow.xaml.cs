using SensorInterface.Model;
using System.Windows;

namespace SensorInterface
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
