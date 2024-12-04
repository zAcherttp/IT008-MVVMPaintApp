using MVVMPaintApp.ViewModels;
using System.Windows;

namespace MVVMPaintApp.Views
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardWindow : Window
    {
        public DashboardWindow(DashboardViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        public DashboardWindow()
        {
            InitializeComponent();
        }
    }
}
