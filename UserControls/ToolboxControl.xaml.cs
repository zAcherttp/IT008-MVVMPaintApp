using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using MVVMPaintApp.Models;
using MVVMPaintApp.Models.Tools;
using MVVMPaintApp.Services;

namespace MVVMPaintApp.UserControls
{
    /// <summary>
    /// Interaction logic for ToolboxControl.xaml
    /// </summary>
    public partial class ToolboxControl : UserControl
    {
        public ToolboxControl()
        {
            InitializeComponent();
        }

        public void Button_PencilTool_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.ToolboxViewModel vm && vm.ProjectManager.SelectedLayer != null)
            {
                vm.ViewModelLocator.DrawingCanvasViewModel.SetTool(ToolType.Pencil);
            }
        }

        public void Button_BrushTool_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.ToolboxViewModel vm && vm.ProjectManager.SelectedLayer != null)
            {
                vm.ViewModelLocator.DrawingCanvasViewModel.SetTool(ToolType.Brush);
            }
        }

        public void Button_EraserTool_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.ToolboxViewModel vm && vm.ProjectManager.SelectedLayer != null)
            {
                vm.ViewModelLocator.DrawingCanvasViewModel.SetTool(ToolType.Eraser);
            }
        }

        public void Button_FillTool_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.ToolboxViewModel vm && vm.ProjectManager.SelectedLayer != null)
            {
                vm.ViewModelLocator.DrawingCanvasViewModel.SetTool(ToolType.Fill);
            }
        }

        public void Button_ColorPickerTool_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.ToolboxViewModel vm && vm.ProjectManager.SelectedLayer != null)
            {
                vm.ViewModelLocator.DrawingCanvasViewModel.SetTool(ToolType.ColorPicker);
            }
        }
    }
}
