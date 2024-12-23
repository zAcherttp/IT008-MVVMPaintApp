using System.Windows.Controls;
using MVVMPaintApp.ViewModels;

namespace MVVMPaintApp.UserControls
{
    /// <summary>
    /// Interaction logic for LayerControl.xaml
    /// </summary>
    public partial class LayerControl : UserControl
    {
        public LayerControl()
        {
            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = (LayerViewModel)DataContext;
            vm?.SelectionChangedCommand.Execute(null);
        }

        private void LayerContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = (LayerViewModel)DataContext;
            if (vm != null)
            {
                vm.MoveLayerUpCommand.NotifyCanExecuteChanged();
                vm.MoveLayerDownCommand.NotifyCanExecuteChanged();
                vm.DeleteLayerCommand.NotifyCanExecuteChanged();
                vm.MergeLayerDownCommand.NotifyCanExecuteChanged();
            }
        }
    }
}
