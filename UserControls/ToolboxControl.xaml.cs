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
    }
}
