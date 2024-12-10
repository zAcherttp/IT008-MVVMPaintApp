using MVVMPaintApp.ViewModels;
using System.Windows;
using System.ComponentModel;
using MVVMPaintApp.Services;

namespace MVVMPaintApp.Views
{
    /// <summary>
    /// Interaction logic for MainCanvasView.xaml
    /// </summary>
    public partial class MainCanvasWindow : Window
    {

        public MainCanvasWindow()
        {
            InitializeComponent();
        }

        public MainCanvasWindow(MainCanvasViewModel mainCanvasViewModel)
        {
            InitializeComponent();
            DataContext = mainCanvasViewModel;
        }

        private void MainCanvasWindow_Closing(object sender, CancelEventArgs e)
        {
            var mainCanvasViewModel = (MainCanvasViewModel)DataContext;
            if (mainCanvasViewModel.HasUnsavedChanges)
            {
                var result = MessageBox.Show("Do you want to save changes?", "Unsaved changes", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    ProjectManager.SaveProject(mainCanvasViewModel.CurrentProject);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
