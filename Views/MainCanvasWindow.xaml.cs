using MVVMPaintApp.ViewModels;
using System.Windows;
using System.ComponentModel;
using MVVMPaintApp.Services;
using System.Windows.Input;
using MVVMPaintApp.UserControls;

namespace MVVMPaintApp.Views
{
    /// <summary>
    /// Interaction logic for MainCanvasView.xaml
    /// </summary>
    public partial class MainCanvasWindow : Window
    {
        private readonly HashSet<Key> currentlyPressedKeys = [];
        private MainCanvasViewModel ViewModel => (MainCanvasViewModel)DataContext;

        public MainCanvasWindow(MainCanvasViewModel mainCanvasViewModel)
        {
            InitializeComponent();
            DataContext = mainCanvasViewModel;
        }

        private void MainCanvasWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ViewModel.DrawingCanvasViewModel != null)
            {
                e.Handled = true;
                if (!currentlyPressedKeys.Contains(e.Key))
                {
                    currentlyPressedKeys.Add(e.Key);
                }

                ViewModel.DrawingCanvasViewModel.HandleKey(e.Key, [.. currentlyPressedKeys]);

                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                {
                    e.Handled = true;
                }

                Cursor = ViewModel.DrawingCanvasViewModel.GetCursor();
            }
        }

        private void MainCanvasWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (ViewModel.DrawingCanvasViewModel != null)
            {
                e.Handled = true;
                currentlyPressedKeys.Remove(e.Key);
            }
        }

        private void MainCanvasWindow_Closing(object sender, CancelEventArgs e)
        {
            if (ViewModel.ProjectManager.HasUnsavedChanges)
            {
                var result = MessageBox.Show("Do you want to save changes?", "Unsaved changes", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    ViewModel.ProjectManager.SaveProject();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
