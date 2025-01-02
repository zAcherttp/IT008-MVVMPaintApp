using MVVMPaintApp.ViewModels;
using System.Windows;
using System.ComponentModel;
using MVVMPaintApp.Services;
using System.Windows.Input;
using MVVMPaintApp.UserControls;
using System.Windows.Controls;
using MVVMPaintApp.Models;
using MVVMPaintApp.Converters;
using Microsoft.Extensions.DependencyInjection;
using MVVMPaintApp.Interfaces;
using System.Diagnostics;

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

                Cursor = ViewModel.DrawingCanvasViewModel.SelectedTool.GetCursor();
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
                var vm = new SaveChangesViewModel();
                var (result, dialogVm) = ViewModel.dialogService.ShowDialog(vm);
                if (result)
                {
                    ViewModel.ProjectManager.SaveProject();
                }
                else if (dialogVm.DialogResult == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
