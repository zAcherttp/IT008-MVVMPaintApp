using MVVMPaintApp.ViewModels;
using System.Windows.Controls;
using MVVMPaintApp.Models;
using System.Windows;

namespace MVVMPaintApp.Views
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            InitializeComponent();
        }

        public DashboardWindow(DashboardViewModel dashboardViewModel)
        {
            InitializeComponent();
            DataContext = dashboardViewModel;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0];
                if (item is SerializableProject project)
                {
                    ((DashboardViewModel)DataContext).SelectedProject = project;
                }
            }
        }
    }
}
