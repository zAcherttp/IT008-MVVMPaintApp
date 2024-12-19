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
            var viewModel = (LayerViewModel)DataContext;
            if (viewModel != null)
            {
                viewModel.SelectionChangedCommand.Execute(null);
            }
        }
    }
}
