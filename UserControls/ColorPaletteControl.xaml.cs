using MVVMPaintApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MVVMPaintApp.UserControls
{
    /// <summary>
    /// Interaction logic for ColorPalette.xaml
    /// </summary>
    public partial class ColorPaletteControl : UserControl
    {
        public ColorPaletteControl()
        {
            InitializeComponent();
            DataContext = new ColorPaletteViewModel(ColorPicker);
        }

        private void OnPrimaryColorChecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is ColorPaletteViewModel vm)
            {
                vm.SetActiveColor(true);
            }
        }

        private void OnSecondaryColorChecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is ColorPaletteViewModel vm)
            {
                vm.SetActiveColor(false);
            }
        }
    }
}
